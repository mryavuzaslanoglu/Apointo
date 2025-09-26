using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Apointo.Domain.Common;
using Apointo.Domain.Staff;

namespace Apointo.Domain.Services;

public sealed class Service : BaseEntity, IAuditableEntity
{
    private readonly List<StaffService> _staffServices = new();

    private Service()
    {
    }

    private Service(
        Guid businessId,
        Guid serviceCategoryId,
        string name,
        string? description,
        decimal price,
        int durationInMinutes,
        int bufferTimeInMinutes,
        bool isActive,
        string? colorHex)
    {
        BusinessId = businessId;
        ServiceCategoryId = serviceCategoryId;
        Name = name;
        Description = description;
        Price = price;
        DurationInMinutes = durationInMinutes;
        BufferTimeInMinutes = bufferTimeInMinutes;
        IsActive = isActive;
        ColorHex = colorHex;
    }

    public Guid BusinessId { get; private set; }
    public Guid ServiceCategoryId { get; private set; }
    public ServiceCategory? Category { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationInMinutes { get; private set; }
    public int BufferTimeInMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public string? ColorHex { get; private set; }

    public IReadOnlyCollection<StaffService> StaffServices => new ReadOnlyCollection<StaffService>(_staffServices);

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static Service Create(
        Guid businessId,
        Guid serviceCategoryId,
        string name,
        string? description,
        decimal price,
        int durationInMinutes,
        int bufferTimeInMinutes,
        bool isActive,
        string? colorHex)
    {
        return new Service(
            businessId,
            serviceCategoryId,
            name,
            description,
            price,
            durationInMinutes,
            bufferTimeInMinutes,
            isActive,
            colorHex);
    }

    public void Update(
        Guid serviceCategoryId,
        string name,
        string? description,
        decimal price,
        int durationInMinutes,
        int bufferTimeInMinutes,
        bool isActive,
        string? colorHex)
    {
        ServiceCategoryId = serviceCategoryId;
        Name = name;
        Description = description;
        Price = price;
        DurationInMinutes = durationInMinutes;
        BufferTimeInMinutes = bufferTimeInMinutes;
        IsActive = isActive;
        ColorHex = colorHex;
    }

    public void AssignStaff(Guid staffId)
    {
        if (_staffServices.Any(x => x.StaffId == staffId))
        {
            return;
        }

        _staffServices.Add(StaffService.Create(staffId, Id));
    }

    public void RemoveStaff(Guid staffId)
    {
        var staffService = _staffServices.FirstOrDefault(x => x.StaffId == staffId);
        if (staffService is not null)
        {
            _staffServices.Remove(staffService);
        }
    }
}
