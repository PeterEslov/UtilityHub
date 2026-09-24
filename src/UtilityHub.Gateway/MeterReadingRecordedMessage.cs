namespace UtilityHub.Gateway;

/// <summary>
/// The Gateway's own copy of the message contract MeterCore publishes.
///
/// Notice this project does NOT reference MeterCore.Api to reuse its type.
/// That's intentional, not an oversight: in a real distributed system,
/// MeterCore and this Function are deployed, versioned and owned separately
/// (possibly by different teams). If the Gateway referenced MeterCore's
/// assembly directly, deploying a change to MeterCore could silently break
/// the Function's build. Duplicating the small message shape here is the
/// price of that independence — it's the same trade-off a shared NuGet
/// "contracts" package tries to solve, just made explicit instead of hidden
/// behind a project reference.
/// </summary>
public record MeterReadingRecordedMessage(
    string MeterId,
    string CustomerId,
    DateTimeOffset Timestamp,
    double ConsumptionKwh,
    bool IsEstimated);
