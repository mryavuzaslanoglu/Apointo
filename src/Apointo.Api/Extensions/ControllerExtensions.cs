using Microsoft.AspNetCore.Mvc;
using Apointo.Api.Contracts;

namespace Apointo.Api.Extensions;

public static class ControllerExtensions
{
    public static ActionResult<ApiResponse<T>> Success<T>(this ControllerBase controller, T data, string message = "Operation successful")
    {
        var response = ApiResponse<T>.SuccessResponse(data, message);
        return controller.Ok(response);
    }

    public static ActionResult<ApiResponse> Success(this ControllerBase controller, string message = "Operation successful")
    {
        var response = ApiResponse.SuccessResponse(message);
        return controller.Ok(response);
    }

    public static ActionResult<ApiResponse<T>> BadRequest<T>(this ControllerBase controller, string message, List<string>? errors = null)
    {
        var response = ApiResponse<T>.ErrorResponse(message, 400, errors);
        return controller.BadRequest(response);
    }

    public static ActionResult<ApiResponse> BadRequest(this ControllerBase controller, string message, List<string>? errors = null)
    {
        var response = ApiResponse.ErrorResponse(message, 400, errors);
        return controller.BadRequest(response);
    }

    public static ActionResult<ApiResponse<T>> NotFound<T>(this ControllerBase controller, string message = "Resource not found")
    {
        var response = ApiResponse<T>.NotFoundResponse(message);
        return controller.NotFound(response);
    }

    public static ActionResult<ApiResponse> NotFound(this ControllerBase controller, string message = "Resource not found")
    {
        var response = ApiResponse.NotFoundResponse(message);
        return controller.NotFound(response);
    }

    public static ActionResult<ApiResponse<T>> Unauthorized<T>(this ControllerBase controller, string message = "Unauthorized")
    {
        var response = ApiResponse<T>.UnauthorizedResponse(message);
        return controller.Unauthorized(response);
    }

    public static ActionResult<ApiResponse> Unauthorized(this ControllerBase controller, string message = "Unauthorized")
    {
        var response = ApiResponse.UnauthorizedResponse(message);
        return controller.Unauthorized(response);
    }

    public static ActionResult<ApiResponse<T>> InternalServerError<T>(this ControllerBase controller, string message = "Internal server error")
    {
        var response = ApiResponse<T>.ErrorResponse(message, 500);
        return controller.StatusCode(500, response);
    }

    public static ActionResult<ApiResponse> InternalServerError(this ControllerBase controller, string message = "Internal server error")
    {
        var response = ApiResponse.ErrorResponse(message, 500);
        return controller.StatusCode(500, response);
    }
}