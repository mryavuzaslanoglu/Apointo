using System.Collections.Generic;
using Apointo.Domain.Common;

namespace Apointo.Domain.Businesses.ValueObjects;

public sealed class Address : ValueObject
{
    private Address()
    {
    }

    private Address(
        string line1,
        string? line2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? State { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    public static Address Create(
        string line1,
        string? line2,
        string city,
        string? state,
        string postalCode,
        string country)
    {
        return new Address(line1, line2, city, state, postalCode, country);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }
}
