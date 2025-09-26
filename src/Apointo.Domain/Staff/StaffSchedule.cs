using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Staff;

public sealed class StaffSchedule : BaseEntity, IAuditableEntity
{
    private StaffSchedule()
    {
    }

    private StaffSchedule(Guid staffId, DayOfWeek dayOfWeek, bool isWorking, TimeSpan? startTime, TimeSpan? endTime)
    {
        StaffId = staffId;
        DayOfWeek = dayOfWeek;
        IsWorking = isWorking;
        StartTime = startTime;
        EndTime = endTime;
    }

    public Guid StaffId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsWorking { get; private set; }
    public TimeSpan? StartTime { get; private set; }
    public TimeSpan? EndTime { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static StaffSchedule Create(Guid staffId, DayOfWeek dayOfWeek, bool isWorking, TimeSpan? startTime, TimeSpan? endTime)
    {
        return new StaffSchedule(staffId, dayOfWeek, isWorking, startTime, endTime);
    }

    public void Update(bool isWorking, TimeSpan? startTime, TimeSpan? endTime)
    {
        IsWorking = isWorking;
        StartTime = startTime;
        EndTime = endTime;
    }
}
