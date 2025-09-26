using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Dtos;
using Apointo.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Commands.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive) : IRequest<Result<ServiceCategoryDto>>;

public sealed class CreateServiceCategoryCommandHandler : IRequestHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServiceCategoryDto>> Handle(CreateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var businessId = await _context.Businesses
            .AsNoTracking()
            .Select(b => b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (businessId == Guid.Empty)
        {
            return Result<ServiceCategoryDto>.Failure("BusinessNotFound");
        }

        var exists = await _context.ServiceCategories
            .AnyAsync(c => c.BusinessId == businessId && c.Name == request.Name, cancellationToken);

        if (exists)
        {
            return Result<ServiceCategoryDto>.Failure("ServiceCategoryNameExists");
        }

        var category = ServiceCategory.Create(businessId, request.Name, request.Description, request.DisplayOrder);
        category.SetActive(request.IsActive);

        await _context.ServiceCategories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ServiceCategoryDto(
            category.Id.ToString(),
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive);

        return Result<ServiceCategoryDto>.Success(dto);
    }
}
