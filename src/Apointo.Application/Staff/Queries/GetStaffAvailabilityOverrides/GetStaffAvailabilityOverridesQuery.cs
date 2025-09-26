using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Queries.GetStaffAvailabilityOverrides;

public sealed record GetStaffAvailabilityOverridesQuery(Guid StaffId) : IRequest<Result<IReadOnlyCollection<StaffAvailabilityOverrideDto>>>;

public sealed class GetStaffAvailabilityOverridesQueryHandler : IRequestHandler<GetStaffAvailabilityOverridesQuery, Result<IReadOnlyCollection<StaffAvailabilityOverrideDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAvailabilityOverridesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<StaffAvailabilityOverrideDto>>> Handle(GetStaffAvailabilityOverridesQuery request, CancellationToken cancellationToken)
    {
        var overrides = await _context.StaffAvailabilityOverrides
            .AsNoTracking()
            .Where(o => o.StaffId == request.StaffId)
            .OrderByDescending(o => o.Date)
            .ToListAsync(cancellationToken);

        if (overrides.Count == 0)
        {
            var staffExists = await _context.StaffMembers
                .AsNoTracking()
                .AnyAsync(s => s.Id == request.StaffId, cancellationToken);

            if (!staffExists)
            {
                return Result<IReadOnlyCollection<StaffAvailabilityOverrideDto>>.Failure("StaffNotFound");
            }
        }

        var dto = overrides
            .Select(o => new StaffAvailabilityOverrideDto(
                o.Id.ToString(),
                o.Date,
                o.Type,
                o.StartTime.HasValue ? o.StartTime.Value.ToString(@"hh\:mm") : null,
                o.EndTime.HasValue ? o.EndTime.Value.ToString(@"hh\:mm") : null,
                o.Reason))
            .ToList();

        return Result<IReadOnlyCollection<StaffAvailabilityOverrideDto>>.Success(dto);
    }
}
