using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Staff;

public sealed class StaffAvailabilityOverride : BaseEntity, IAuditableEntity
{
    private StaffAvailabilityOverride()
    {
    }

    private StaffAvailabilityOverride(
        Guid staffId,
        DateOnly date,
        StaffAvailabilityType type,
        TimeSpan? startTime,
        TimeSpan? endTime,
        string? reason)
    {
        StaffId = staffId;
        Date = date;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason;
    }

    public Guid StaffId { get; private set; }
    public DateOnly Date { get; private set; }
    public StaffAvailabilityType Type { get; private set; }
    public TimeSpan? StartTime { get; private set; }
    public TimeSpan? EndTime { get; private set; }
    public string? Reason { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static StaffAvailabilityOverride Create(
        Guid staffId,
        DateOnly date,
        StaffAvailabilityType type,
        TimeSpan? startTime,
        TimeSpan? endTime,
        string? reason)
    {
        return new StaffAvailabilityOverride(staffId, date, type, startTime, endTime, reason);
    }

    public void Update(StaffAvailabilityType type, TimeSpan? startTime, TimeSpan? endTime, string? reason)
    {
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason;
    }
}
