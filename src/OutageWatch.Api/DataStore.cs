using Bogus;

namespace OutageWatch.Api;

public class OutageStore
{
    private readonly List<Outage> _outages = new();
    private readonly object _lock = new();

    public OutageStore()
    {
        var areaCodes = new[] { "ESLOV-N", "ESLOV-S", "LUND-C", "LANDSKRONA-V", "HELSINGBORG-O" };

        var faker = new Faker<Outage>()
            .RuleFor(o => o.AreaCode, f => f.PickRandom(areaCodes))
            .RuleFor(o => o.StartedAt, f => f.Date.RecentOffset(14))
            .RuleFor(o => o.ResolvedAt, (f, o) => f.Random.Bool(0.7f) ? o.StartedAt.AddMinutes(f.Random.Int(15, 480)) : null)
            .RuleFor(o => o.Severity, f => f.PickRandom<OutageSeverity>())
            .RuleFor(o => o.Description, f => f.Lorem.Sentence(6));

        _outages.AddRange(faker.Generate(40));
    }

    public IReadOnlyList<Outage> GetAll() => _outages.AsReadOnly();

    public IEnumerable<Outage> GetActive() => _outages.Where(o => o.IsActive);

    public Outage Add(CreateOutageRequest request)
    {
        var outage = new Outage
        {
            AreaCode = request.AreaCode,
            StartedAt = DateTimeOffset.UtcNow,
            Severity = request.Severity,
            Description = request.Description
        };

        lock (_lock)
        {
            _outages.Add(outage);
        }

        return outage;
    }
}
