using System;
using Apointo.Domain.Common;
using Apointo.Domain.Services;

namespace Apointo.Domain.Staff;

public sealed class StaffService : BaseEntity, IAuditableEntity
{
    private StaffService()
    {
    }

    private StaffService(Guid staffId, Guid serviceId)
    {
        StaffId = staffId;
        ServiceId = serviceId;
    }

    public Guid StaffId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Staff? Staff { get; private set; }
    public Service? Service { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static StaffService Create(Guid staffId, Guid serviceId)
    {
        return new StaffService(staffId, serviceId);
    }
}
