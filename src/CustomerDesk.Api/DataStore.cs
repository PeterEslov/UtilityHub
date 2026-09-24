using Bogus;

namespace CustomerDesk.Api;

public class CustomerDeskStore
{
    private readonly List<Customer> _customers = new();
    private readonly List<CaseRecord> _cases = new();
    private readonly object _lock = new();

    public CustomerDeskStore()
    {
        var customerIds = Enumerable.Range(1, 25).Select(i => $"CUST-{i:D5}").ToArray();

        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Address, f => $"{f.Address.StreetAddress()}, {f.Address.ZipCode()} {f.Address.City()}")
            .RuleFor(c => c.ContactEmail, f => f.Internet.Email());

        _customers.AddRange(customerFaker.Generate(25).DistinctBy(c => c.CustomerId));

        var caseFaker = new Faker<CaseRecord>()
            .RuleFor(c => c.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(c => c.Subject, f => f.PickRandom(
                "Fråga om faktura", "Flyttanmälan", "Avbrott anmält", "Ändra autogiro", "Klagomål mätaravläsning"))
            .RuleFor(c => c.Status, f => f.PickRandom<CaseStatus>())
            .RuleFor(c => c.CreatedAt, f => f.Date.RecentOffset(30));

        _cases.AddRange(caseFaker.Generate(60));
    }

    public IReadOnlyList<Customer> GetCustomers() => _customers.AsReadOnly();

    public IReadOnlyList<CaseRecord> GetCases() => _cases.AsReadOnly();

    public CaseRecord AddCase(CreateCaseRequest request)
    {
        var record = new CaseRecord
        {
            CustomerId = request.CustomerId,
            Subject = request.Subject,
            Status = CaseStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow
        };

        lock (_lock)
        {
            _cases.Add(record);
        }

        return record;
    }
}
