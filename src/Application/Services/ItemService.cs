using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Features.Items;

public record CreateItemCommand(int ProductId, int Quantity) : IRequest<Result<ItemDto>>;

public record UpdateItemCommand(int Id, int ProductId, int Quantity) : IRequest<Result<ItemDto>>;

public record DeleteItemCommand(int Id, int ProductId) : IRequest<Result>;

public record GetItemsByProductIdQuery(int ProductId) : IRequest<Result<IEnumerable<ItemDto>>>;

public record GetItemByIdQuery(int Id) : IRequest<Result<ItemDto>>;

public class ItemCommandHandler :
    IRequestHandler<CreateItemCommand, Result<ItemDto>>,
    IRequestHandler<UpdateItemCommand, Result<ItemDto>>,
    IRequestHandler<DeleteItemCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ItemDto>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result<ItemDto>.Failure($"Product with ID {request.ProductId} was not found.", 404);
        }

        var item = new Item
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ItemDto>(item);
        return Result<ItemDto>.Success(dto, "Item added to product successfully.", 201);
    }

    public async Task<Result<ItemDto>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result<ItemDto>.Failure($"Product with ID {request.ProductId} was not found.", 404);
        }

        var item = await _unitOfWork.Items.GetByIdAsync(request.Id, cancellationToken);
        if (item == null || item.ProductId != request.ProductId)
        {
            return Result<ItemDto>.Failure($"Item with ID {request.Id} was not found under product {request.ProductId}.", 404);
        }

        item.Quantity = request.Quantity;
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ItemDto>(item);
        return Result<ItemDto>.Success(dto, "Item quantity updated successfully.", 200);
    }

    public async Task<Result> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure($"Product with ID {request.ProductId} was not found.", 404);
        }

        var item = await _unitOfWork.Items.GetByIdAsync(request.Id, cancellationToken);
        if (item == null || item.ProductId != request.ProductId)
        {
            return Result.Failure($"Item with ID {request.Id} was not found under product {request.ProductId}.", 404);
        }

        _unitOfWork.Items.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Item deleted successfully from product.", 200);
    }
}

public class ItemQueryHandler :
    IRequestHandler<GetItemsByProductIdQuery, Result<IEnumerable<ItemDto>>>,
    IRequestHandler<GetItemByIdQuery, Result<ItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ItemDto>>> Handle(GetItemsByProductIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result<IEnumerable<ItemDto>>.Failure($"Product with ID {request.ProductId} was not found.", 404);
        }

        var items = await _unitOfWork.Items.GetItemsByProductIdAsync(request.ProductId, cancellationToken);
        var dtos = _mapper.Map<IEnumerable<ItemDto>>(items);

        return Result<IEnumerable<ItemDto>>.Success(dtos, "Items retrieved successfully.", 200);
    }

    public async Task<Result<ItemDto>> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(request.Id, cancellationToken);
        if (item == null)
        {
            return Result<ItemDto>.Failure($"Item with ID {request.Id} was not found.", 404);
        }

        var dto = _mapper.Map<ItemDto>(item);
        return Result<ItemDto>.Success(dto, "Item retrieved successfully.", 200);
    }
}
