using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Common;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Features.Products;

namespace ProductManagement.API.Controllers.v1
{
    /// <summary>
    /// Controller managing Product resources (Version 1).
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products")]
    [Authorize] // All Product endpoints require authentication
    public class ProductsController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public ProductsController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new Product.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a paginated, sorted, and filtered list of Products.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedList<ProductDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sort = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetProductsQuery(pageNumber, pageSize, search, sort);
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Retrieves a Product by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates a Product's details.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(int id, [FromBody] string productName, CancellationToken cancellationToken)
        {
            var command = new UpdateProductCommand(id, productName);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a Product. Restricted to Admin role only.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")] // Only users in the Admin role can execute Delete operations
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            return HandleResult(result);
        }
    }
}
