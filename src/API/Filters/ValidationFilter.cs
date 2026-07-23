using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductManagement.API.Common;

namespace ProductManagement.API.Filters
{
    /// <summary>
    /// Action filter that intercepts validation failures and converts them to standard 422 Unprocessable Entity responses.
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errorMessages = context.ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                var apiResponse = new ApiResponse(
                    success: false,
                    message: "Validation failed.",
                    statusCode: StatusCodes.Status422UnprocessableEntity,
                    data: null,
                    errors: errorMessages
                );

                context.Result = new UnprocessableEntityObjectResult(apiResponse);
                return;
            }

            await next();
        }
    }
}
