using AutoMapper;
using Apointo.Api.Contracts.BusinessSettings;
using Apointo.Application.Businesses.Commands.UpdateBusinessSettings;
using Apointo.Application.Businesses.Queries.GetBusinessSettings;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/business")]
[Authorize(Policy = "RequireAdminRole")]
public sealed class BusinessController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public BusinessController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(BusinessSettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessSettingsResponse>> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBusinessSettingsQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<BusinessSettingsResponse>(result.Value!);
        return Ok(response);
    }

    [HttpPut("settings")]
    [ProducesResponseType(typeof(BusinessSettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessSettingsResponse>> UpdateSettings(
        [FromBody] UpdateBusinessSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<UpdateBusinessSettingsCommand>(request);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return HandleFailure(result.ToNonGeneric());
        }

        var response = _mapper.Map<BusinessSettingsResponse>(result.Value!);
        return Ok(response);
    }


    private ActionResult HandleFailure(Result result)
    {
        return result.Error switch
        {
            "BusinessNotFound" => NotFound(CreateProblemDetails(result.Error, "Business kaydý bulunamadý.", StatusCodes.Status404NotFound)),
            "InvalidOperatingHours" => BadRequest(CreateProblemDetails(result.Error, "Çalýþma saatleri geçersiz.", StatusCodes.Status400BadRequest)),
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



