namespace MeterCore.Api;

/// <summary>
/// The message MeterCore publishes to Service Bus when a reading is recorded.
///
/// Notice this is a SEPARATE type from <see cref="MeterReading"/>, not the
/// same record reused. That's deliberate: the internal domain model
/// (MeterReading) is free to change — add fields, rename things — without
/// breaking every downstream consumer of the queue. The message contract is
/// the "public API" of this integration and should only change deliberately,
/// with consumers in mind. Coupling a queue message directly to an internal
/// EF Core entity is a classic way migrations quietly break other systems.
/// </summary>
public record MeterReadingRecordedMessage(
    string MeterId,
    string CustomerId,
    DateTimeOffset Timestamp,
    double ConsumptionKwh,
    bool IsEstimated);
