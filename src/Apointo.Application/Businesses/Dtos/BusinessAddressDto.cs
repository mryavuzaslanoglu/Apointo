namespace Apointo.Application.Businesses.Dtos;

public sealed record BusinessAddressDto(
    string? Line1,
    string? Line2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country);
