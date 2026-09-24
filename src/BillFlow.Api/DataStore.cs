using Bogus;

namespace BillFlow.Api;

public class InvoiceStore
{
    private readonly List<Invoice> _invoices = new();
    private readonly object _lock = new();

    public InvoiceStore()
    {
        var customerIds = Enumerable.Range(1, 25).Select(i => $"CUST-{i:D5}").ToArray();

        var faker = new Faker<Invoice>()
            .RuleFor(i => i.CustomerId, f => f.PickRandom(customerIds))
            .RuleFor(i => i.PeriodStart, f => DateOnly.FromDateTime(f.Date.Past(1)))
            .RuleFor(i => i.PeriodEnd, (f, i) => i.PeriodStart.AddMonths(1))
            .RuleFor(i => i.AmountSek, f => Math.Round((decimal)f.Random.Double(400, 4500), 2))
            .RuleFor(i => i.Status, f => f.PickRandom<InvoiceStatus>());

        _invoices.AddRange(faker.Generate(150));
    }

    public IReadOnlyList<Invoice> GetAll() => _invoices.AsReadOnly();

    public IEnumerable<Invoice> GetByCustomerId(string customerId) =>
        _invoices.Where(i => i.CustomerId.Equals(customerId, StringComparison.OrdinalIgnoreCase));

    public Invoice Add(CreateInvoiceRequest request)
    {
        var invoice = new Invoice
        {
            CustomerId = request.CustomerId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            AmountSek = request.AmountSek,
            Status = InvoiceStatus.Draft
        };

        lock (_lock)
        {
            _invoices.Add(invoice);
        }

        return invoice;
    }
}
