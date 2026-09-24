using Bogus;

namespace MeterCore.Api;

/// <summary>
/// In-memory store seeded with synthetic data via Bogus at startup.
/// Stands in for a real database until EF Core + Azure SQL is wired up
/// (see the UtilityHub architecture spec, "Syntetisk datagenerator").
/// Deliberately includes a few "messy" edge cases (estimated readings,
/// occasional gaps) so downstream consumers have something real to handle.
/// </summary>
public class MeterReadingStore
{
    private readonly List<MeterReading> _readings = new();
    private readonly object _lock = new();

    public MeterReadingStore()
    {
        var meterIds = Enumerable.Range(1, 25).Select(i => $"MTR-{i:D5}").ToArray();
        var customerIds = Enumerable.Range(1, 25).Select(i => $"CUST-{i:D5}").ToArray();

        var faker = new Faker<MeterReading>()
            .RuleFor(r => r.MeterId, f => f.PickRandom(meterIds))
            .RuleFor(r => r.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(r => r.Timestamp, f => f.Date.RecentOffset(30))
            .RuleFor(r => r.ConsumptionKwh, f => Math.Round(f.Random.Double(0.1, 15.0), 2))
            .RuleFor(r => r.IsEstimated, f => f.Random.Bool(0.08f)); // ~8% estimated, like real meter data

        _readings.AddRange(faker.Generate(500));
    }

    public IReadOnlyList<MeterReading> GetAll() => _readings.AsReadOnly();

    public IEnumerable<MeterReading> GetByMeterId(string meterId) =>
        _readings.Where(r => r.MeterId.Equals(meterId, StringComparison.OrdinalIgnoreCase));

    public MeterReading Add(CreateMeterReadingRequest request)
    {
        var reading = new MeterReading
        {
            MeterId = request.MeterId,
            CustomerId = request.CustomerId,
            Timestamp = DateTimeOffset.UtcNow,
            ConsumptionKwh = request.ConsumptionKwh,
            IsEstimated = request.IsEstimated
        };

        lock (_lock)
        {
            _readings.Add(reading);
        }

        return reading;
    }
}
