using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Simulation.Models;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationFeatureLoader : ISimulationFeatureLoader
{
    private readonly SimulationOptions _options;
    private readonly ILogger<SimulationFeatureLoader> _logger;
    private Dictionary<int, List<FeatureRecord>>? _cachedFeatures;

    public SimulationFeatureLoader(IOptions<SimulationOptions> options, ILogger<SimulationFeatureLoader> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SimulationZoneFeatures>> LoadHourlyFeaturesAsync(
        DateTime simulatedTime,
        int zoneCount,
        CancellationToken ct = default)
    {
        var hour = simulatedTime.Hour;
        var records = await TryLoadFromFileAsync(hour, ct) ?? GenerateSyntheticFeatures(simulatedTime, zoneCount);
        return records.Select(record => BuildZoneFeatures(record, simulatedTime)).ToList();
    }

    private async Task<List<FeatureRecord>?> TryLoadFromFileAsync(int hour, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.FeatureDataPath))
        {
            return null;
        }

        if (_cachedFeatures is null)
        {
            if (!File.Exists(_options.FeatureDataPath))
            {
                _logger.LogWarning("Simulation feature data file not found at {Path}", _options.FeatureDataPath);
                return null;
            }

            await using var stream = File.OpenRead(_options.FeatureDataPath);
            var dataset = await JsonSerializer.DeserializeAsync<Dictionary<int, List<FeatureRecord>>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);

            _cachedFeatures = dataset ?? new Dictionary<int, List<FeatureRecord>>();
        }

        return _cachedFeatures.TryGetValue(hour, out var records) ? records : null;
    }

    private static List<FeatureRecord> GenerateSyntheticFeatures(DateTime simulatedTime, int zoneCount)
    {
        var hour = simulatedTime.Hour;
        var dayOfWeek = (int)simulatedTime.DayOfWeek;
        var isWeekend = dayOfWeek is 0 or 6;
        var hourFactor = 0.75 + 0.5 * Math.Sin((hour / 24.0) * Math.PI * 2 - Math.PI / 2);
        var rainMm = hour is >= 15 and <= 18 ? 1.4 : 0.2;
        var tempC = 10 + 10 * Math.Sin((hour / 24.0) * Math.PI * 2);
        var weatherCode = rainMm > 1 ? 61 : 1;
        var records = new List<FeatureRecord>(zoneCount);

        for (var zoneId = 1; zoneId <= zoneCount; zoneId++)
        {
            var zoneFactor = 0.8 + (zoneId % 10) * 0.04;
            var baseDemand = Math.Max(2, 28 * hourFactor * zoneFactor);
            records.Add(new FeatureRecord
            {
                ZoneId = zoneId,
                BaseDemand = baseDemand,
                BaseRevenue = baseDemand * (10 + (zoneId % 5)),
                AvgFare = 12 + (zoneId % 5),
                TipRate = 0.08 + (zoneId % 3) * 0.02,
                RainMm = rainMm,
                TempC = tempC,
                WeatherCode = weatherCode,
                DayOfWeek = dayOfWeek,
                IsWeekend = isWeekend
            });
        }

        return records;
    }

    private static SimulationZoneFeatures BuildZoneFeatures(FeatureRecord record, DateTime simulatedTime)
    {
        var hour = simulatedTime.Hour;
        var month = simulatedTime.Month;
        var timeBucket6h = new DateTime(simulatedTime.Year, simulatedTime.Month, simulatedTime.Day, hour - (hour % 6), 0, 0, DateTimeKind.Utc);
        var demandInput = new Demand6hInput(
            record.ZoneId,
            hour,
            record.DayOfWeek,
            record.IsWeekend,
            false,
            record.BaseDemand * 0.8,
            record.BaseDemand * 0.6,
            record.BaseDemand * 0.4,
            record.BaseDemand,
            record.TempC,
            record.RainMm,
            record.RainMm > 0.5,
            record.WeatherCode,
            (int)Math.Round(record.BaseDemand));

        var revenueInput = new RevenueInput(
            record.ZoneId,
            hour,
            record.DayOfWeek,
            record.IsWeekend,
            (int)Math.Round(record.BaseDemand * 0.7),
            (int)Math.Round(record.BaseDemand * 0.5),
            (int)Math.Round(record.BaseDemand * 0.3),
            record.BaseRevenue * 0.7,
            record.BaseRevenue * 0.6,
            record.BaseRevenue * 0.4,
            record.BaseRevenue * 0.3,
            (decimal?)(record.BaseRevenue / 24),
            record.AvgFare,
            record.TipRate,
            record.TempC,
            record.RainMm,
            record.RainMm > 0.5,
            record.WeatherCode,
            false);

        var stockInput = new StockOutInput(
            record.ZoneId,
            timeBucket6h,
            record.BaseDemand,
            record.BaseDemand * 0.9,
            record.BaseDemand * 0.1,
            hour,
            record.DayOfWeek,
            record.IsWeekend,
            false,
            Math.Clamp(record.BaseDemand / 50, 0.1, 1.0),
            record.TempC,
            record.RainMm,
            record.RainMm > 0.5,
            record.BaseDemand * 0.7,
            record.BaseDemand * 0.6,
            record.BaseDemand * 0.1,
            record.WeatherCode);

        var etaInput = new ETAInput(
            record.ZoneId,
            record.ZoneId,
            (decimal?)record.TempC,
            (decimal?)record.RainMm,
            record.WeatherCode,
            2 + record.ZoneId % 4,
            hour,
            record.DayOfWeek,
            month,
            0,
            record.IsWeekend,
            hour is >= 7 and <= 10 || hour is >= 16 and <= 19,
            new DateTime(simulatedTime.Year, simulatedTime.Month, simulatedTime.Day, hour, 0, 0, DateTimeKind.Utc),
            "short",
            600,
            600,
            1.1m,
            600);

        return new SimulationZoneFeatures
        {
            ZoneId = record.ZoneId,
            DemandInput = demandInput,
            RevenueInput = revenueInput,
            StockOutInput = stockInput,
            EtaInput = etaInput
        };
    }

    private sealed class FeatureRecord
    {
        public int ZoneId { get; init; }
        public double BaseDemand { get; init; }
        public double BaseRevenue { get; init; }
        public double AvgFare { get; init; }
        public double TipRate { get; init; }
        public double RainMm { get; init; }
        public double TempC { get; init; }
        public int WeatherCode { get; init; }
        public int DayOfWeek { get; init; }
        public bool IsWeekend { get; init; }
    }
}
