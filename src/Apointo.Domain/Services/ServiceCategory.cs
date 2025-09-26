using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Apointo.Domain.Common;

namespace Apointo.Domain.Services;

public sealed class ServiceCategory : BaseEntity, IAuditableEntity
{
    private readonly List<Service> _services = new();

    private ServiceCategory()
    {
    }

    private ServiceCategory(Guid businessId, string name, string? description, int displayOrder)
    {
        BusinessId = businessId;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
    }

    public Guid BusinessId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<Service> Services => new ReadOnlyCollection<Service>(_services);

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static ServiceCategory Create(Guid businessId, string name, string? description, int displayOrder)
    {
        return new ServiceCategory(businessId, name, description, displayOrder);
    }

    public void Update(string name, string? description, int displayOrder, bool isActive)
    {
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
