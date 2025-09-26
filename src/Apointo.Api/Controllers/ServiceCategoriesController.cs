using System;
using System.Collections.Generic;
using System.Linq;
using Apointo.Api.Contracts.Services;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Commands.CreateServiceCategory;
using Apointo.Application.ServiceCatalog.Commands.DeleteServiceCategory;
using Apointo.Application.ServiceCatalog.Commands.UpdateServiceCategory;
using Apointo.Application.ServiceCatalog.Dtos;
using Apointo.Application.ServiceCatalog.Queries.GetServiceCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/service-categories")]
[Authorize(Policy = "RequireAdminRole")]
public sealed class ServiceCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ServiceCategoryResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ServiceCategoryResponse>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetServiceCategoriesQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = result.Value!
            .Select(MapToResponse)
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceCategoryResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ServiceCategoryResponse>> CreateCategory(
        [FromBody] CreateServiceCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCategoryCommand(
            request.Name,
            request.Description,
            request.DisplayOrder,
            request.IsActive);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = MapToResponse(result.Value!);
        return CreatedAtAction(nameof(GetCategories), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServiceCategoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceCategoryResponse>> UpdateCategory(
        Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.DisplayOrder,
            request.IsActive);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        return Ok(MapToResponse(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteServiceCategoryCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    private static ServiceCategoryResponse MapToResponse(ServiceCategoryDto dto)
    {
        return new ServiceCategoryResponse(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.DisplayOrder,
            dto.IsActive);
    }

    private ActionResult HandleFailure(Result result)
    {
        return result.Error switch
        {
            "BusinessNotFound" => NotFound(CreateProblemDetails(result.Error, "Ýþletme kaydý bulunamadý.", StatusCodes.Status404NotFound)),
            "ServiceCategoryNotFound" => NotFound(CreateProblemDetails(result.Error, "Hizmet kategorisi bulunamadý.", StatusCodes.Status404NotFound)),
            "ServiceCategoryNameExists" => Conflict(CreateProblemDetails(result.Error, "Bu isimde bir kategori zaten mevcut.", StatusCodes.Status409Conflict)),
            "ServiceCategoryHasServices" => BadRequest(CreateProblemDetails(result.Error, "Kategoriye baðlý hizmetler bulunduðu için silinemiyor.", StatusCodes.Status400BadRequest)),
            _ => Problem(detail: "Beklenmeyen bir hata oluþtu.")
        };
    }

    private static ProblemDetails CreateProblemDetails(string? errorCode, string detail, int statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Title = detail,
            Detail = detail,
            Status = statusCode
        };

        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            problemDetails.Extensions["code"] = errorCode;
        }

        return problemDetails;
    }
}
