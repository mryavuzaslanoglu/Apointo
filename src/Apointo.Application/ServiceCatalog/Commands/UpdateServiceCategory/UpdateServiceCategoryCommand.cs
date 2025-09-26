using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Commands.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive) : IRequest<Result<ServiceCategoryDto>>;

public sealed class UpdateServiceCategoryCommandHandler : IRequestHandler<UpdateServiceCategoryCommand, Result<ServiceCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServiceCategoryDto>> Handle(UpdateServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result<ServiceCategoryDto>.Failure("ServiceCategoryNotFound");
        }

        var exists = await _context.ServiceCategories
            .AnyAsync(c => c.BusinessId == category.BusinessId && c.Name == request.Name && c.Id != request.CategoryId, cancellationToken);

        if (exists)
        {
            return Result<ServiceCategoryDto>.Failure("ServiceCategoryNameExists");
        }

        category.Update(request.Name, request.Description, request.DisplayOrder, request.IsActive);

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
