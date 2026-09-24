namespace CustomerDesk.Api;

public enum CaseStatus
{
    Open,
    InProgress,
    Resolved
}

public record Customer
{
    public required string CustomerId { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string ContactEmail { get; init; }
}

/// <summary>
/// A support case. In the "kund flyttar" saga, a new case here is one of the
/// steps a Durable Functions orchestration coordinates alongside BillFlow
/// and MeterCore — and one of the first things rolled back if a later step
/// fails.
/// </summary>
public record CaseRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerId { get; init; }
    public required string Subject { get; init; }
    public CaseStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateCaseRequest(string CustomerId, string Subject);
