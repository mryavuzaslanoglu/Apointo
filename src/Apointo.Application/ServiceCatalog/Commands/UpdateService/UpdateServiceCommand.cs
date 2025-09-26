using System;
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

namespace Apointo.Application.ServiceCatalog.Commands.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    string Name,
    string? Description,
    decimal Price,
    int DurationInMinutes,
    int BufferTimeInMinutes,
    bool IsActive,
    string? ColorHex,
    Guid CategoryId,
    IReadOnlyCollection<Guid> StaffIds) : IRequest<Result<ServiceDto>>;

public sealed class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, Result<ServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServiceDto>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .Include(s => s.StaffServices)
            .ThenInclude(ss => ss.Staff)
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            return Result<ServiceDto>.Failure("ServiceNotFound");
        }

        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result<ServiceDto>.Failure("ServiceCategoryNotFound");
        }

        var nameExists = await _context.Services
            .AnyAsync(s => s.BusinessId == service.BusinessId && s.Name == request.Name && s.Id != request.ServiceId, cancellationToken);

        if (nameExists)
        {
            return Result<ServiceDto>.Failure("ServiceNameExists");
        }

        var staffMembers = await _context.StaffMembers
            .Where(s => request.StaffIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (staffMembers.Count != request.StaffIds.Count)
        {
            return Result<ServiceDto>.Failure("ServiceStaffInvalid");
        }

        if (staffMembers.Any(s => s.BusinessId != service.BusinessId))
        {
            return Result<ServiceDto>.Failure("ServiceStaffInvalid");
        }

        service.Update(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.DurationInMinutes,
            request.BufferTimeInMinutes,
            request.IsActive,
            request.ColorHex);

        var requestedStaff = request.StaffIds.ToHashSet();
        var existingStaff = service.StaffServices.Select(ss => ss.StaffId).ToHashSet();

        foreach (var staffId in existingStaff.Except(requestedStaff).ToList())
        {
            service.RemoveStaff(staffId);
        }

        foreach (var staffId in requestedStaff.Except(existingStaff))
        {
            service.AssignStaff(staffId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var assignedStaffDtos = staffMembers
            .Where(s => requestedStaff.Contains(s.Id))
            .Select(s => new StaffSummaryDto(
                s.Id.ToString(),
                s.FullName,
                s.FirstName,
                s.LastName,
                s.Title,
                s.Email,
                s.PhoneNumber,
                s.IsActive))
            .ToList();

        var dto = new ServiceDto(
            service.Id.ToString(),
            service.Name,
            service.Description,
            service.Price,
            service.DurationInMinutes,
            service.BufferTimeInMinutes,
            service.IsActive,
            service.ColorHex,
            service.ServiceCategoryId.ToString(),
            category.Name,
            assignedStaffDtos);

        return Result<ServiceDto>.Success(dto);
    }
}
