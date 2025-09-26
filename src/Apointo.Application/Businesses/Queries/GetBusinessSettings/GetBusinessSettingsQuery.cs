using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Businesses.Dtos;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Businesses.Queries.GetBusinessSettings;

public sealed record GetBusinessSettingsQuery : IRequest<Result<BusinessSettingsDto>>;

public sealed class GetBusinessSettingsQueryHandler : IRequestHandler<GetBusinessSettingsQuery, Result<BusinessSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBusinessSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BusinessSettingsDto>> Handle(GetBusinessSettingsQuery request, CancellationToken cancellationToken)
    {
        var business = await _context.Businesses
            .AsNoTracking()
            .Include(b => b.OperatingHours)
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
        {
            return Result<BusinessSettingsDto>.Failure("BusinessNotFound");
        }

        var addressDto = business.Address is null
            ? null
            : new BusinessAddressDto(
                business.Address.Line1,
                business.Address.Line2,
                business.Address.City,
                business.Address.State,
                business.Address.PostalCode,
                business.Address.Country);

        var hours = business.OperatingHours
            .OrderBy(h => h.DayOfWeek)
            .Select(h => new BusinessOperatingHourDto(
                h.DayOfWeek,
                h.IsClosed,
                h.OpenTime?.ToString(@"hh\:mm"),
                h.CloseTime?.ToString(@"hh\:mm")))
            .ToList();

        var result = new BusinessSettingsDto(
            business.Id.ToString(),
            business.Name,
            business.Description,
            business.PhoneNumber,
            business.Email,
            business.WebsiteUrl,
            addressDto,
            hours);

        return Result<BusinessSettingsDto>.Success(result);
    }
}
