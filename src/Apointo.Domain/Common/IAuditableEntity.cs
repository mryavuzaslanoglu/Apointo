using System;

namespace Apointo.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime? LastModifiedAtUtc { get; set; }
    Guid? LastModifiedBy { get; set; }
}