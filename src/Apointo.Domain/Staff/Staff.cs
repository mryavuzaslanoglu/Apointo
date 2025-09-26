using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Apointo.Domain.Common;

namespace Apointo.Domain.Staff;

public sealed class Staff : BaseEntity, IAuditableEntity
{
    private readonly List<StaffSchedule> _schedules = new();
    private readonly List<StaffAvailabilityOverride> _availabilityOverrides = new();
    private readonly List<StaffService> _staffServices = new();

    private Staff()
    {
    }

    private Staff(
        Guid businessId,
        string firstName,
        string lastName,
        string? email,
        string? phoneNumber,
        string? title,
        Guid? userId)
    {
        BusinessId = businessId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Title = title;
        UserId = userId;
    }

    public Guid BusinessId { get; private set; }
    public Guid? UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Title { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? HiredAtUtc { get; private set; }
    public DateTime? TerminatedAtUtc { get; private set; }

    public IReadOnlyCollection<StaffSchedule> Schedules => new ReadOnlyCollection<StaffSchedule>(_schedules);
    public IReadOnlyCollection<StaffAvailabilityOverride> AvailabilityOverrides => new ReadOnlyCollection<StaffAvailabilityOverride>(_availabilityOverrides);
    public IReadOnlyCollection<StaffService> StaffServices => new ReadOnlyCollection<StaffService>(_staffServices);

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public static Staff Create(
        Guid businessId,
        string firstName,
        string lastName,
        string? email,
        string? phoneNumber,
        string? title,
        Guid? userId,
        DateTime? hiredAtUtc)
    {
        return new Staff(businessId, firstName, lastName, email, phoneNumber, title, userId)
        {
            HiredAtUtc = hiredAtUtc
        };
    }

    public void Update(
        string firstName,
        string lastName,
        string? email,
        string? phoneNumber,
        string? title,
        bool isActive,
        Guid? userId,
        DateTime? hiredAtUtc,
        DateTime? terminatedAtUtc)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Title = title;
        IsActive = isActive;
        UserId = userId;
        HiredAtUtc = hiredAtUtc;
        TerminatedAtUtc = terminatedAtUtc;
    }

    public void SetSchedules(IEnumerable<StaffSchedule> schedules)
    {
        _schedules.Clear();
        _schedules.AddRange(schedules.OrderBy(x => x.DayOfWeek));
    }

    public void SetAvailabilityOverrides(IEnumerable<StaffAvailabilityOverride> overrides)
    {
        _availabilityOverrides.Clear();
        _availabilityOverrides.AddRange(overrides.OrderBy(x => x.Date));
    }

    public void AssignService(Guid serviceId)
    {
        if (_staffServices.Any(x => x.ServiceId == serviceId))
        {
            return;
        }

        _staffServices.Add(StaffService.Create(Id, serviceId));
    }

    public void RemoveService(Guid serviceId)
    {
        var staffService = _staffServices.FirstOrDefault(x => x.ServiceId == serviceId);
        if (staffService is not null)
        {
            _staffServices.Remove(staffService);
        }
    }
}
