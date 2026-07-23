using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Common;
using ProductManagement.Application.Common;

namespace ProductManagement.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult(Result result)
    {
        var response = new ApiResponse(
            success: result.IsSuccess,
            message: result.Message,
            statusCode: result.StatusCode,
            data: null,
            errors: result.Errors
        );

        return StatusCode(result.StatusCode, response);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        var response = new ApiResponse<T>(
            success: result.IsSuccess,
            message: result.Message,
            statusCode: result.StatusCode,
            data: result.Value,
            errors: result.Errors
        );

        return StatusCode(result.StatusCode, response);
    }
}

