using System.Collections.Generic;
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
using ProductManagement.Application.Features.Items;

namespace ProductManagement.API.Controllers.v1;

public class ItemRequestModel
{
    public int Quantity { get; set; }
}

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products/{productId:int}/items")]
[Authorize]
public class ItemsController : ApiControllerBase
{
    private readonly ISender _mediator;

    public ItemsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(int productId, [FromBody] ItemRequestModel model, CancellationToken cancellationToken)
    {
        var command = new CreateItemCommand(productId, model.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProductId(int productId, CancellationToken cancellationToken)
    {
        var query = new GetItemsByProductIdQuery(productId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int productId, int id, CancellationToken cancellationToken)
    {
        var query = new GetItemByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsSuccess && result.Value != null && result.Value.ProductId != productId)
        {
            return HandleResult(Result<ItemDto>.Failure($"Item with ID {id} does not belong to product {productId}.", 404));
        }
        
        return HandleResult(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int productId, int id, [FromBody] ItemRequestModel model, CancellationToken cancellationToken)
    {
        var command = new UpdateItemCommand(id, productId, model.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int productId, int id, CancellationToken cancellationToken)
    {
        var command = new DeleteItemCommand(id, productId);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

