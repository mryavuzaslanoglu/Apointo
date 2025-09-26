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

namespace Apointo.Application.Staff.Queries.GetStaffSchedule;

public sealed record GetStaffScheduleQuery(Guid StaffId) : IRequest<Result<IReadOnlyCollection<StaffScheduleDto>>>;

public sealed class GetStaffScheduleQueryHandler : IRequestHandler<GetStaffScheduleQuery, Result<IReadOnlyCollection<StaffScheduleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffScheduleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<StaffScheduleDto>>> Handle(GetStaffScheduleQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _context.StaffSchedules
            .AsNoTracking()
            .Where(s => s.StaffId == request.StaffId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(cancellationToken);

        if (schedules.Count == 0)
        {
            var staffExists = await _context.StaffMembers
                .AsNoTracking()
                .AnyAsync(s => s.Id == request.StaffId, cancellationToken);

            if (!staffExists)
            {
                return Result<IReadOnlyCollection<StaffScheduleDto>>.Failure("StaffNotFound");
            }
        }

        var dto = schedules
            .Select(s => new StaffScheduleDto(
                s.DayOfWeek,
                s.IsWorking,
                s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : null,
                s.EndTime.HasValue ? s.EndTime.Value.ToString(@"hh\:mm") : null))
            .ToList();

        return Result<IReadOnlyCollection<StaffScheduleDto>>.Success(dto);
    }
}
