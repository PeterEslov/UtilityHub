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

public record CreateMeterReadingRequest(string MeterId, string CustomerId, double ConsumptionKwh, bool IsEstimated = false)
{
    /// <summary>
    /// Minimal APIs don't run DataAnnotations validation automatically the
    /// way MVC controllers with [ApiController] do — there's no free "400 if
    /// a field is missing". This is the explicit, readable alternative:
    /// collect every problem found (not just the first one) into the shape
    /// Results.ValidationProblem() expects, so a caller gets ALL their
    /// mistakes back in one response instead of fixing them one at a time.
    /// </summary>
    public IDictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(MeterId))
        {
            errors[nameof(MeterId)] = new[] { "MeterId får inte vara tomt." };
        }

        if (string.IsNullOrWhiteSpace(CustomerId))
        {
            errors[nameof(CustomerId)] = new[] { "CustomerId får inte vara tomt." };
        }

        if (ConsumptionKwh < 0)
        {
            errors[nameof(ConsumptionKwh)] = new[] { "ConsumptionKwh kan inte vara negativt." };
        }

        return errors;
    }
}
