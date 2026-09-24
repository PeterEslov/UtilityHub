namespace BillFlow.Api;

/// <summary>
/// Holds usage records that have arrived but not yet been rolled into an
/// invoice. In a later phase, a scheduled job (or another Function) would
/// aggregate these per customer/period and turn them into an Invoice —
/// that aggregation step is out of scope for Fas 1.
/// </summary>
public class UsageStore
{
    private readonly List<UsageRecord> _records = new();
    private readonly object _lock = new();

    public IReadOnlyList<UsageRecord> GetAll()
    {
        lock (_lock)
        {
            return _records.ToList();
        }
    }

    public UsageRecord Add(CreateUsageRecordRequest request)
    {
        var record = new UsageRecord
        {
            CustomerId = request.CustomerId,
            SourceMeterId = request.SourceMeterId,
            ConsumptionKwh = request.ConsumptionKwh,
            RecordedAt = request.RecordedAt
        };

        lock (_lock)
        {
            _records.Add(record);
        }

        return record;
    }
}
