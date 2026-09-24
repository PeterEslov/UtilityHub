namespace MeterCore.Api;

/// <summary>
/// A single meter reading. Mirrors the shape of a real mätvärdesinsamlingssystem
/// closely enough to be a realistic integration source, without touching any
/// real production data.
/// </summary>
public record MeterReading
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string MeterId { get; init; }
    public required string CustomerId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public double ConsumptionKwh { get; init; }
    public bool IsEstimated { get; init; }
}

public record CreateMeterReadingRequest(string MeterId, string CustomerId, double ConsumptionKwh, bool IsEstimated = false);
