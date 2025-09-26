using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Queries.GetStaffList;

public sealed record GetStaffListQuery : IRequest<Result<IReadOnlyCollection<StaffSummaryDto>>>;

public sealed class GetStaffListQueryHandler : IRequestHandler<GetStaffListQuery, Result<IReadOnlyCollection<StaffSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<StaffSummaryDto>>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffMembers
            .AsNoTracking()
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new StaffSummaryDto(
                s.Id.ToString(),
                string.Join(" ", new[] { s.FirstName, s.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                s.FirstName,
                s.LastName,
                s.Title,
                s.Email,
                s.PhoneNumber,
                s.IsActive))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<StaffSummaryDto>>.Success(staff);
    }
}
