using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Apointo.Api.Contracts.Staff;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Commands.CreateAvailabilityOverride;
using Apointo.Application.Staff.Commands.CreateStaff;
using Apointo.Application.Staff.Commands.DeleteAvailabilityOverride;
using Apointo.Application.Staff.Commands.DeleteStaff;
using Apointo.Application.Staff.Commands.UpdateStaff;
using Apointo.Application.Staff.Commands.UpdateStaffSchedule;
using Apointo.Application.Staff.Queries.GetStaffAvailabilityOverrides;
using Apointo.Application.Staff.Queries.GetStaffById;
using Apointo.Application.Staff.Queries.GetStaffList;
using Apointo.Application.Staff.Queries.GetStaffSchedule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Policy = "RequireAdminRole")]
public sealed class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public StaffController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(StaffSummaryResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<StaffSummaryResponse>>> GetStaff(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffListQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<StaffSummaryResponse>>(result.Value!);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StaffResponse>> GetStaffById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffByIdQuery(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        return Ok(_mapper.Map<StaffResponse>(result.Value!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StaffResponse>> CreateStaff(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateStaffCommand(
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.PhoneNumber,
            request.IsActive,
            ParseNullableGuid(request.UserId),
            request.HiredAtUtc);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<StaffResponse>(result.Value!);
        return CreatedAtAction(nameof(GetStaffById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StaffResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StaffResponse>> UpdateStaff(
        Guid id,
        [FromBody] UpdateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStaffCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.PhoneNumber,
            request.IsActive,
            ParseNullableGuid(request.UserId),
            request.HiredAtUtc,
            request.TerminatedAtUtc);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        return Ok(_mapper.Map<StaffResponse>(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteStaff(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteStaffCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(StaffScheduleResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<StaffScheduleResponse>>> GetSchedule(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffScheduleQuery(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<StaffScheduleResponse>>(result.Value!);

        return Ok(response);
    }

    [HttpPut("{id:guid}/schedule")]
    [ProducesResponseType(typeof(StaffScheduleResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<StaffScheduleResponse>>> UpdateSchedule(
        Guid id,
        [FromBody] UpdateStaffScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStaffScheduleCommand(
            id,
            request.Schedules.Select(s => new StaffScheduleInput(s.DayOfWeek, s.IsWorking, s.StartTime, s.EndTime)).ToList());

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<StaffScheduleResponse>>(result.Value!);

        return Ok(response);
    }

    [HttpGet("{id:guid}/availability-overrides")]
    [ProducesResponseType(typeof(StaffAvailabilityOverrideResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<StaffAvailabilityOverrideResponse>>> GetAvailabilityOverrides(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStaffAvailabilityOverridesQuery(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<StaffAvailabilityOverrideResponse>>(result.Value!);

        return Ok(response);
    }

    [HttpPost("{id:guid}/availability-override")]
    [ProducesResponseType(typeof(StaffAvailabilityOverrideResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StaffAvailabilityOverrideResponse>> CreateAvailabilityOverride(
        Guid id,
        [FromBody] CreateStaffAvailabilityOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateStaffAvailabilityOverrideCommand(
            id,
            request.Date,
            request.Type,
            request.StartTime,
            request.EndTime,
            request.Reason);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<StaffAvailabilityOverrideResponse>(result.Value!);

        return CreatedAtAction(nameof(GetAvailabilityOverrides), new { id }, response);
    }

    [HttpDelete("{id:guid}/availability-override/{overrideId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAvailabilityOverride(Guid id, Guid overrideId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteStaffAvailabilityOverrideCommand(id, overrideId), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    private static Guid? ParseNullableGuid(string? value)
    {
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private ActionResult HandleFailure(Result result)
    {
        return result.Error switch
        {
            "BusinessNotFound" => NotFound(CreateProblemDetails(result.Error, "Ýþletme kaydý bulunamadý.", StatusCodes.Status404NotFound)),
            "StaffNotFound" => NotFound(CreateProblemDetails(result.Error, "Personel bulunamadý.", StatusCodes.Status404NotFound)),
            "StaffEmailAlreadyExists" => Conflict(CreateProblemDetails(result.Error, "Bu e-posta adresi baþka bir personel tarafýndan kullanýlýyor.", StatusCodes.Status409Conflict)),
            "AvailabilityOverrideExists" => Conflict(CreateProblemDetails(result.Error, "Belirtilen tarih için kayýtlý bir izin kaydý mevcut.", StatusCodes.Status409Conflict)),
            "AvailabilityOverrideNotFound" => NotFound(CreateProblemDetails(result.Error, "Ýstenen izin kaydý bulunamadý.", StatusCodes.Status404NotFound)),
            "AvailabilityEndBeforeStart" => BadRequest(CreateProblemDetails(result.Error, "Bitiþ saati baþlangýç saatinden sonra olmalýdýr.", StatusCodes.Status400BadRequest)),
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









