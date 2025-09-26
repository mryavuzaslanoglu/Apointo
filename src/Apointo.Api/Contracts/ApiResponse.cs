namespace Apointo.Api.Contracts;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404
        };
    }

    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 401
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = 200
        };
    }

    public new static ApiResponse ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>()
        };
    }

    public new static ApiResponse NotFoundResponse(string message = "Resource not found")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 404
        };
    }

    public new static ApiResponse UnauthorizedResponse(string message = "Unauthorized")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            StatusCode = 401
        };
    }
}