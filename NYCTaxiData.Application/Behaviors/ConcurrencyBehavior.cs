using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Behaviors
{
    public class ConcurrencyBehavior<TRequest, TResponse>
     : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private readonly ILogger<ConcurrencyBehavior<TRequest, TResponse>> _logger;

        public ConcurrencyBehavior(
            ILogger<ConcurrencyBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // ✅ بس شتغل على الـ Commands مش الـ Queries
            if (request is not ITransactionalCommand)
                return await next();

            try
            {
                return await next();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.FirstOrDefault();
                var entityName = entry?.Entity.GetType().Name ?? "Entity";
                var entityId = entry?.Property("Id").CurrentValue ?? "Unknown";

                _logger.LogWarning(
                    "[Concurrency] Conflict on {Entity} with ID {Id} | Command: {Command}",
                    entityName, entityId, typeof(TRequest).Name);

                throw new ConcurrencyException(entityName, entityId);
            }
        }
    }
}
