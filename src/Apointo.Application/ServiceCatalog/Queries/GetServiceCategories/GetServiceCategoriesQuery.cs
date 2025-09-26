using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Queries.GetServiceCategories;

public sealed record GetServiceCategoriesQuery : IRequest<Result<IReadOnlyCollection<ServiceCategoryDto>>>;

public sealed class GetServiceCategoriesQueryHandler : IRequestHandler<GetServiceCategoriesQuery, Result<IReadOnlyCollection<ServiceCategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<ServiceCategoryDto>>> Handle(GetServiceCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.ServiceCategories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ServiceCategoryDto(
                c.Id.ToString(),
                c.Name,
                c.Description,
                c.DisplayOrder,
                c.IsActive))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<ServiceCategoryDto>>.Success(categories);
    }
}
