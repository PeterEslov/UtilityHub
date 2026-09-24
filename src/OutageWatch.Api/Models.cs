namespace OutageWatch.Api;

public enum OutageSeverity
{
    Minor,
    Major,
    Critical
}

/// <summary>
/// A registered outage/disturbance. This is the event source that
/// Event Grid publishes onward to interested subscribers (e.g. CustomerDesk).
/// </summary>
public record Outage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string AreaCode { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public OutageSeverity Severity { get; init; }
    public required string Description { get; init; }

    public bool IsActive => ResolvedAt is null;
}

public record CreateOutageRequest(string AreaCode, OutageSeverity Severity, string Description);
