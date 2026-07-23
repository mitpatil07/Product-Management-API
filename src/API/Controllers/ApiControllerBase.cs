using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Common;
using ProductManagement.Application.Common;

namespace ProductManagement.API.Controllers
{
    /// <summary>
    /// Base API Controller providing common utilities for translating MediatR results into HTTP status responses.
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Translates a Result (non-generic) into a standardized IActionResult.
        /// </summary>
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

        /// <summary>
        /// Translates a generic Result of type T into a standardized IActionResult containing the generic payload.
        /// </summary>
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
}
