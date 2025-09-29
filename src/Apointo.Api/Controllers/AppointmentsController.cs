using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Api.Contracts.Appointments;
using Apointo.Application.Appointments.Commands.CancelAppointment;
using Apointo.Application.Appointments.Commands.CreateAppointment;
using Apointo.Application.Appointments.Commands.UpdateAppointment;
using Apointo.Application.Appointments.Queries.FindAvailableSlots;
using Apointo.Application.Appointments.Queries.GetCalendarAppointments;
using Apointo.Application.Appointments.Queries.GetCustomerAppointments;
using Apointo.Application.Common.Models;
using Apointo.Domain.Appointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AppointmentsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost("find-available-slots")]
    [ProducesResponseType(typeof(FindAvailableSlotsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FindAvailableSlotsResponse>> FindAvailableSlots(
        [FromBody] FindAvailableSlotsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseGuids(request.ServiceIds, out var serviceIds))
        {
            return BadRequest(CreateProblemDetails("InvalidServiceIds", "Invalid service IDs.", StatusCodes.Status400BadRequest));
        }

        Guid? preferredStaffId = null;
        if (!string.IsNullOrEmpty(request.PreferredStaffId))
        {
            if (!Guid.TryParse(request.PreferredStaffId, out var staffId))
            {
                return BadRequest(CreateProblemDetails("InvalidStaffId", "Invalid staff ID.", StatusCodes.Status400BadRequest));
            }
            preferredStaffId = staffId;
        }

        var businessId = GetBusinessIdFromClaims();
        var query = new FindAvailableSlotsQuery(
            serviceIds,
            preferredStaffId,
            request.StartDate,
            request.EndDate,
            businessId);

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<FindAvailableSlotsResponse>(result.Value!);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseGuids(request.ServiceIds, out var serviceIds))
        {
            return BadRequest(CreateProblemDetails("InvalidServiceIds", "Invalid service IDs.", StatusCodes.Status400BadRequest));
        }

        if (!Guid.TryParse(request.StaffId, out var staffId))
        {
            return BadRequest(CreateProblemDetails("InvalidStaffId", "Invalid staff ID.", StatusCodes.Status400BadRequest));
        }

        var businessId = GetBusinessIdFromClaims();
        var customerId = GetUserIdFromClaims();

        var command = new CreateAppointmentCommand(
            businessId,
            customerId,
            staffId,
            request.StartTimeUtc,
            serviceIds,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<AppointmentResponse>(result.Value!);
        return CreatedAtAction(nameof(GetAppointment), new { id = response.Id }, response);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(AppointmentResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AppointmentResponse>>> GetMyAppointments(
        [FromQuery] GetCustomerAppointmentsRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetUserIdFromClaims();
        var query = new GetCustomerAppointmentsQuery(
            customerId,
            request.IncludePast,
            request.PageNumber,
            request.PageSize);

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<IReadOnlyCollection<AppointmentResponse>>(result.Value!);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    public ActionResult<AppointmentResponse> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        // This would need a GetAppointmentByIdQuery implementation
        return NotFound();
    }

    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelAppointment(
        Guid id,
        [FromBody] CancelAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        var command = new CancelAppointmentCommand(id, userId, request.CancellationReason);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAppointment(
        Guid id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        Guid? newStaffId = null;
        if (!string.IsNullOrEmpty(request.NewStaffId))
        {
            if (!Guid.TryParse(request.NewStaffId, out var staffId))
            {
                return BadRequest(CreateProblemDetails("InvalidStaffId", "Invalid staff ID.", StatusCodes.Status400BadRequest));
            }
            newStaffId = staffId;
        }

        AppointmentStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (!Enum.TryParse<AppointmentStatus>(request.Status, out var appointmentStatus))
            {
                return BadRequest(CreateProblemDetails("InvalidStatus", "Invalid appointment status.", StatusCodes.Status400BadRequest));
            }
            status = appointmentStatus;
        }

        var command = new UpdateAppointmentCommand(
            id,
            request.NewStartTimeUtc,
            request.NewEndTimeUtc,
            newStaffId,
            request.Notes,
            status);

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpGet("calendar")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(CalendarViewResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CalendarViewResponse>> GetCalendarAppointments(
        [FromQuery] GetCalendarAppointmentsRequest request,
        CancellationToken cancellationToken)
    {
        List<Guid>? staffIds = null;
        if (request.StaffIds?.Any() == true && !TryParseGuids(request.StaffIds, out staffIds))
        {
            return BadRequest(CreateProblemDetails("InvalidStaffIds", "Invalid staff IDs.", StatusCodes.Status400BadRequest));
        }

        var businessId = GetBusinessIdFromClaims();
        var query = new GetCalendarAppointmentsQuery(
            businessId,
            request.StartDate,
            request.EndDate,
            staffIds);

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<CalendarViewResponse>(result.Value!);
        return Ok(response);
    }

    private Guid GetBusinessIdFromClaims()
    {
        var businessIdClaim = User.FindFirst("business_id")?.Value;
        if (string.IsNullOrEmpty(businessIdClaim) || !Guid.TryParse(businessIdClaim, out var businessId))
        {
            throw new UnauthorizedAccessException("Business ID not found in claims");
        }
        return businessId;
    }

    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
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
            "ServicesRequired" => BadRequest(CreateProblemDetails(result.Error, "At least one service must be selected.", StatusCodes.Status400BadRequest)),
            "InvalidDateRange" => BadRequest(CreateProblemDetails(result.Error, "Invalid date range.", StatusCodes.Status400BadRequest)),
            "SomeServicesNotFound" => NotFound(CreateProblemDetails(result.Error, "Some services were not found.", StatusCodes.Status404NotFound)),
            "NoEligibleStaff" => BadRequest(CreateProblemDetails(result.Error, "No staff available for selected services.", StatusCodes.Status400BadRequest)),
            "BusinessNotFound" => NotFound(CreateProblemDetails(result.Error, "Business not found.", StatusCodes.Status404NotFound)),
            "StaffNotFound" => NotFound(CreateProblemDetails(result.Error, "Staff member not found.", StatusCodes.Status404NotFound)),
            "StaffCannotPerformAllServices" => BadRequest(CreateProblemDetails(result.Error, "Selected staff cannot perform all services.", StatusCodes.Status400BadRequest)),
            "TimeSlotNotAvailable" => Conflict(CreateProblemDetails(result.Error, "Selected time slot is not available.", StatusCodes.Status409Conflict)),
            "AppointmentNotFound" => NotFound(CreateProblemDetails(result.Error, "Appointment not found.", StatusCodes.Status404NotFound)),
            "UnauthorizedAccess" => Forbid(),
            "CannotCancelCompletedAppointment" => BadRequest(CreateProblemDetails(result.Error, "Cannot cancel completed appointment.", StatusCodes.Status400BadRequest)),
            "AppointmentAlreadyCancelled" => BadRequest(CreateProblemDetails(result.Error, "Appointment is already cancelled.", StatusCodes.Status400BadRequest)),
            "CancellationTooLate" => BadRequest(CreateProblemDetails(result.Error, "Cannot cancel appointment less than 24 hours before.", StatusCodes.Status400BadRequest)),
            _ => Problem(detail: "An unexpected error occurred.")
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