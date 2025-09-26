using System;
using System.Collections.Generic;
using Apointo.Api.Contracts.Staff;

namespace Apointo.Api.Contracts.Services;

public sealed record ServiceCategoryResponse(
    string Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);

public sealed record CreateServiceCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);

public sealed record UpdateServiceCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);

public sealed record ServiceResponse(
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
    IReadOnlyCollection<StaffSummaryResponse> AssignedStaff);

public sealed record CreateServiceRequest(
    string Name,
    string? Description,
    decimal Price,
    int DurationInMinutes,
    int BufferTimeInMinutes,
    bool IsActive,
    string? ColorHex,
    string CategoryId,
    IReadOnlyCollection<string> StaffIds);

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    decimal Price,
    int DurationInMinutes,
    int BufferTimeInMinutes,
    bool IsActive,
    string? ColorHex,
    string CategoryId,
    IReadOnlyCollection<string> StaffIds);
