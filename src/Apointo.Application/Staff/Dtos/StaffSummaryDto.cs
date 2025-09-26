namespace Apointo.Application.Staff.Dtos;

public sealed record StaffSummaryDto(
    string Id,
    string FullName,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive);
