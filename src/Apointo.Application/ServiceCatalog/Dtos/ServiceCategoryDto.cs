namespace Apointo.Application.ServiceCatalog.Dtos;

public sealed record ServiceCategoryDto(
    string Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);
