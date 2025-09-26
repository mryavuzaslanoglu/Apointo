using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Dtos;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Queries.GetServices;

public sealed record GetServicesQuery : IRequest<Result<IReadOnlyCollection<ServiceDto>>>;

public sealed class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, Result<IReadOnlyCollection<ServiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<ServiceDto>>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.StaffServices)
                .ThenInclude(ss => ss.Staff)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(
                s.Id.ToString(),
                s.Name,
                s.Description,
                s.Price,
                s.DurationInMinutes,
                s.BufferTimeInMinutes,
                s.IsActive,
                s.ColorHex,
                s.ServiceCategoryId.ToString(),
                s.Category != null ? s.Category.Name : string.Empty,
                s.StaffServices
                    .Where(ss => ss.Staff != null)
                    .Select(ss => new StaffSummaryDto(
                        ss.Staff!.Id.ToString(),
                        ss.Staff.FullName,
                        ss.Staff.FirstName,
                        ss.Staff.LastName,
                        ss.Staff.Title,
                        ss.Staff.Email,
                        ss.Staff.PhoneNumber,
                        ss.Staff.IsActive))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<ServiceDto>>.Success(services);
    }
}
