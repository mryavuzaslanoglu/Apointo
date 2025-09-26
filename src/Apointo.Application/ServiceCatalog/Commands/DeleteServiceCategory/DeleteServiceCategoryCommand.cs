using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Commands.DeleteServiceCategory;

public sealed record DeleteServiceCategoryCommand(Guid CategoryId) : IRequest<Result>;

public sealed class DeleteServiceCategoryCommandHandler : IRequestHandler<DeleteServiceCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteServiceCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure("ServiceCategoryNotFound");
        }

        var hasServices = await _context.Services
            .AnyAsync(s => s.ServiceCategoryId == category.Id, cancellationToken);

        if (hasServices)
        {
            return Result.Failure("ServiceCategoryHasServices");
        }

        _context.ServiceCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
