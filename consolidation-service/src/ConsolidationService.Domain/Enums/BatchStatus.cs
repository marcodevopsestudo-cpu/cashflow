namespace ConsolidationService.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a consolidation batch.
/// </summary>
public enum BatchStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Ignored = 5
}
