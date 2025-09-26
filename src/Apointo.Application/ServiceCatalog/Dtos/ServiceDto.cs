using System.Collections.Generic;
using Apointo.Application.Staff.Dtos;

namespace Apointo.Application.ServiceCatalog.Dtos;

public sealed record ServiceDto(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationInMinutes,
    int BufferTimeInMinutes,
    bool IsActive,
    string? ColorHex,
    string CategoryId,
    string CategoryName,
    IReadOnlyCollection<StaffSummaryDto> AssignedStaff);
