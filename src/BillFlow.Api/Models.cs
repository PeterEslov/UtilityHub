namespace BillFlow.Api;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue
}

/// <summary>
/// A customer invoice. Mirrors the "transform-and-forward" shape from the
/// real Agresso-integration analysis: a simple record with no orchestration
/// needs of its own — the orchestration lives one layer up, in the flows
/// that call this API.
/// </summary>
public record Invoice
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerId { get; init; }
    public DateOnly PeriodStart { get; init; }
    public DateOnly PeriodEnd { get; init; }
    public decimal AmountSek { get; init; }
    public InvoiceStatus Status { get; init; }
}

public record CreateInvoiceRequest(string CustomerId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal AmountSek);
