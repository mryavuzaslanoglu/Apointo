using AutoMapper;
using System;
using System.Collections.Generic;
using Apointo.Api.Contracts.Services;
using Apointo.Api.Contracts.Staff;
using Apointo.Application.Common.Models;
using Apointo.Application.ServiceCatalog.Commands.CreateService;
using Apointo.Application.ServiceCatalog.Commands.DeleteService;
using Apointo.Application.ServiceCatalog.Commands.UpdateService;
using Apointo.Application.ServiceCatalog.Queries.GetServiceById;
using Apointo.Application.ServiceCatalog.Queries.GetServices;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize(Policy = "RequireAdminRole")]
public sealed class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ServicesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ServiceResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ServiceResponse>>> GetServices(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetServicesQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<ServiceResponse>>(result.Value!);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse>> GetService(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetServiceByIdQuery(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        return Ok(_mapper.Map<ServiceResponse>(result.Value!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ServiceResponse>> CreateService(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseGuids(request.StaffIds, out var staffIds))
        {
            return BadRequest(CreateProblemDetails("InvalidStaffIds", "Geçersiz personel listesi.", StatusCodes.Status400BadRequest));
        }

        if (!Guid.TryParse(request.CategoryId, out var categoryId))
        {
            return BadRequest(CreateProblemDetails("InvalidCategoryId", "Geçersiz kategori kimliði.", StatusCodes.Status400BadRequest));
        }

        var command = new CreateServiceCommand(
            request.Name,
            request.Description,
            request.Price,
            request.DurationInMinutes,
            request.BufferTimeInMinutes,
            request.IsActive,
            request.ColorHex,
            categoryId,
            staffIds);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<ServiceResponse>(result.Value!);
        return CreatedAtAction(nameof(GetService), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse>> UpdateService(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseGuids(request.StaffIds, out var staffIds))
        {
            return BadRequest(CreateProblemDetails("InvalidStaffIds", "Geçersiz personel listesi.", StatusCodes.Status400BadRequest));
        }

        if (!Guid.TryParse(request.CategoryId, out var categoryId))
        {
            return BadRequest(CreateProblemDetails("InvalidCategoryId", "Geçersiz kategori kimliði.", StatusCodes.Status400BadRequest));
        }

        var command = new UpdateServiceCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.DurationInMinutes,
            request.BufferTimeInMinutes,
            request.IsActive,
            request.ColorHex,
            categoryId,
            staffIds);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        return Ok(_mapper.Map<ServiceResponse>(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteService(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteServiceCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    private static bool TryParseGuids(IEnumerable<string> values, out List<Guid> parsed)
    {
        parsed = new List<Guid>();
        foreach (var value in values)
        {
            if (!Guid.TryParse(value, out var guid))
            {
                parsed = new List<Guid>();
                return false;
            }

            parsed.Add(guid);
        }

        return true;
    }

    private ActionResult HandleFailure(Result result)
    {
        return result.Error switch
        {
            "BusinessNotFound" => NotFound(CreateProblemDetails(result.Error, "Ýþletme kaydý bulunamadý.", StatusCodes.Status404NotFound)),
            "ServiceNotFound" => NotFound(CreateProblemDetails(result.Error, "Hizmet bulunamadý.", StatusCodes.Status404NotFound)),
            "ServiceCategoryNotFound" => NotFound(CreateProblemDetails(result.Error, "Hizmet kategorisi bulunamadý.", StatusCodes.Status404NotFound)),
            "ServiceNameExists" => Conflict(CreateProblemDetails(result.Error, "Bu isimde bir hizmet zaten mevcut.", StatusCodes.Status409Conflict)),
            "ServiceStaffInvalid" => BadRequest(CreateProblemDetails(result.Error, "Seçilen personel listesi geçersiz.", StatusCodes.Status400BadRequest)),
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



