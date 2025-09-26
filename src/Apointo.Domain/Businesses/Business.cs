using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Apointo.Domain.Businesses.ValueObjects;
using Apointo.Domain.Common;

namespace Apointo.Domain.Businesses;

public sealed class Business : BaseEntity, IAuditableEntity
{
    private readonly List<BusinessOperatingHour> _operatingHours = new();

    private Business()
    {
    }

    private Business(
        string name,
        string? description,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        Address? address)
    {
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        Address = address;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public Address? Address { get; private set; }

    public IReadOnlyCollection<BusinessOperatingHour> OperatingHours => new ReadOnlyCollection<BusinessOperatingHour>(_operatingHours);

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public static Business Create(
        string name,
        string? description,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        Address? address,
        IEnumerable<BusinessOperatingHour>? operatingHours = null)
    {
        var business = new Business(name, description, phoneNumber, email, websiteUrl, address);
        if (operatingHours is not null)
        {
            business.SetOperatingHours(operatingHours);
        }

        return business;
    }

    public void Update(
        string name,
        string? description,
        string? phoneNumber,
        string? email,
        string? websiteUrl,
        Address? address,
        IEnumerable<BusinessOperatingHour>? operatingHours)
    {
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        WebsiteUrl = websiteUrl;
        Address = address;

        if (operatingHours is not null)
        {
            SetOperatingHours(operatingHours);
        }
    }

    private void SetOperatingHours(IEnumerable<BusinessOperatingHour> operatingHours)
    {
        _operatingHours.Clear();
        _operatingHours.AddRange(operatingHours.OrderBy(x => x.DayOfWeek));
    }
}
