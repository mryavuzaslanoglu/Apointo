using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Dtos;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Queries.GetServiceById;

public sealed record GetServiceByIdQuery(Guid ServiceId) : IRequest<Result<ServiceDto>>;

public sealed class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, Result<ServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServiceDto>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.StaffServices)
                .ThenInclude(ss => ss.Staff)
            .Where(s => s.Id == request.ServiceId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (service is null)
        {
            return Result<ServiceDto>.Failure("ServiceNotFound");
        }

        return Result<ServiceDto>.Success(service);
    }
}
