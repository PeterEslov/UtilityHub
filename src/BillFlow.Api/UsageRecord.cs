namespace BillFlow.Api;

/// <summary>
/// A piece of unbilled usage, forwarded from MeterCore via the Gateway
/// function. This is intentionally NOT the same shape as MeterCore's
/// MeterReadingRecordedMessage — BillFlow only cares about what it needs for
/// billing (customer, amount, when), not meter-specific fields like
/// IsEstimated. Translating between the two shapes is exactly the "transform"
/// half of transform-and-forward, and it happens in the Gateway function, not
/// here or in MeterCore.
/// </summary>
public record UsageRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerId { get; init; }
    public required string SourceMeterId { get; init; }
    public double ConsumptionKwh { get; init; }
    public DateTimeOffset RecordedAt { get; init; }
    public DateTimeOffset ReceivedAt { get; init; } = DateTimeOffset.UtcNow;
}

public record CreateUsageRecordRequest(string CustomerId, string SourceMeterId, double ConsumptionKwh, DateTimeOffset RecordedAt)
{
    public IDictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(CustomerId))
        {
            errors[nameof(CustomerId)] = new[] { "CustomerId får inte vara tomt." };
        }

        if (string.IsNullOrWhiteSpace(SourceMeterId))
        {
            errors[nameof(SourceMeterId)] = new[] { "SourceMeterId får inte vara tomt." };
        }

        if (ConsumptionKwh < 0)
        {
            errors[nameof(ConsumptionKwh)] = new[] { "ConsumptionKwh kan inte vara negativt." };
        }

        return errors;
    }
}
