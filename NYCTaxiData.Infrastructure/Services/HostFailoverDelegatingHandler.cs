using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// A high-availability delegating handler that intercepts AI prediction requests
/// and automatically performs client-side host failovers when servers are down or unresponsive.
/// </summary>
public class HostFailoverDelegatingHandler : DelegatingHandler
{
    private readonly List<string> _hosts;
    private readonly bool _failoverEnabled;
    private readonly ILogger<HostFailoverDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostFailoverDelegatingHandler"/> class.
    /// </summary>
    public HostFailoverDelegatingHandler(IConfiguration configuration, ILogger<HostFailoverDelegatingHandler> logger)
    {
        _logger = logger;
        
        // 1. Read the toggle flag (defaults to true)
        _failoverEnabled = configuration.GetValue<bool>("AiPredictionFailoverEnabled", true);

        // 2. Read the hosts array
        var hostsConfig = configuration.GetSection("AiPredictionHosts").Get<List<string>>();
        _hosts = (hostsConfig ?? new List<string>())
            .Select(h => h.Trim().TrimEnd('/'))
            .Where(h => !string.IsNullOrEmpty(h))
            .ToList();

        // 3. Fallback to default hosts if the configuration is completely missing
        if (!_hosts.Any())
        {
            _hosts.Add("http://www.smart-fleet.me:8000");
            _hosts.Add("https://ai-driven-ride-optimization.onrender.com/");
        }

        _logger.LogInformation("AI High-Availability Failover initialized. Failover Enabled: {Enabled}. Configured Hosts: {Hosts}", 
            _failoverEnabled, string.Join(", ", _hosts));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri == null)
        {
            throw new ArgumentException("Request URI cannot be null for AI prediction failover.");
        }

        // If failover is explicitly disabled or we only have a single host, bypass the failover loop entirely
        if (!_failoverEnabled || _hosts.Count <= 1)
        {
            if (_hosts.Count > 0)
            {
                var targetHost = _hosts[0];
                _logger.LogInformation("AI Failover is disabled. Directing request directly to primary host: {Host}", targetHost);
                request.RequestUri = ReplaceAuthority(request.RequestUri, targetHost);
            }
            return await base.SendAsync(request, cancellationToken);
        }

        Exception? lastException = null;
        HttpResponseMessage? lastResponse = null;

        for (int i = 0; i < _hosts.Count; i++)
        {
            var host = _hosts[i] ?? "";
            var targetUri = ReplaceAuthority(request.RequestUri, host);

            // Clone request to support subsequent retry attempts (a request message can only be sent once in .NET)
            using var clonedRequest = await CloneHttpRequestMessageAsync(request, targetUri);
            _logger.LogInformation("Attempting AI prediction request using host: {Host} (Attempt {Attempt}/{Total})", host, i + 1, _hosts.Count);

            try
            {
                var response = await base.SendAsync(clonedRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("AI prediction request SUCCEEDED on host: {Host}", host);
                    return response;
                }

                // Identify if the error code is a server/transient error eligible for failover
                if (IsFailoverEligible(response.StatusCode))
                {
                    _logger.LogWarning("AI prediction request FAILED on host: {Host} with HTTP Status Code: {StatusCode}. Initiating failover.", host, response.StatusCode);
                    lastResponse = response;
                }
                else
                {
                    // For client-side errors (400, 422, etc.), return immediately as they represent bad payloads
                    _logger.LogWarning("AI prediction request returned client-level status: {StatusCode} on host: {Host}. Bypassing failover.", response.StatusCode, host);
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI prediction request encountered network/timeout error on host: {Host}. Initiating failover.", host);
                lastException = ex;
            }
        }

        // All configured hosts failed
        _logger.LogError("ALL configured AI prediction hosts failed. High-availability fallback exhausted.");
        if (lastResponse != null) return lastResponse;
        if (lastException != null) throw new HttpRequestException("All configured AI prediction hosts failed.", lastException);
        throw new HttpRequestException("All configured AI prediction hosts failed to respond.");
    }

    private static Uri ReplaceAuthority(Uri? originalUri, string targetHost)
    {
        if (originalUri == null)
        {
            throw new ArgumentException("Request URI cannot be null for authority replacement.");
        }
        var baseUri = new Uri(targetHost);
        var targetUriBuilder = new UriBuilder(originalUri)
        {
            Scheme = baseUri.Scheme,
            Host = baseUri.Host,
            Port = baseUri.Port
        };
        return targetUriBuilder.Uri;
    }

    private static bool IsFailoverEligible(HttpStatusCode statusCode)
    {
        int code = (int)statusCode;
        // Server errors (5xx) or request timeout (408) are eligible
        return code >= 500 || code == 408;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, Uri targetUri)
    {
        var clone = new HttpRequestMessage(request.Method, targetUri) { Version = request.Version };
        
        // Copy standard headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy context properties/options
        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        // Copy content stream safely to support payload reuse
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
