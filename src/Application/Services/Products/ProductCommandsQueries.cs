using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Features.Products;

public record CreateProductCommand(string ProductName) : IRequest<Result<ProductDto>>;

public record UpdateProductCommand(int Id, string ProductName) : IRequest<Result<ProductDto>>;

public record DeleteProductCommand(int Id) : IRequest<Result>;

public record GetProductByIdQuery(int Id) : IRequest<Result<ProductDto>>;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    string? Sort = null) : IRequest<Result<PagedList<ProductDto>>>;

public class ProductCommandHandler :
    IRequestHandler<CreateProductCommand, Result<ProductDto>>,
    IRequestHandler<UpdateProductCommand, Result<ProductDto>>,
    IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            ProductName = request.ProductName
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto, "Product created successfully.", 201);
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found.", 404);
        }

        product.ProductName = request.ProductName;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto, "Product updated successfully.", 200);
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure("Product not found.", 404);
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Product deleted successfully.", 200);
    }
}

public class ProductQueryHandler :
    IRequestHandler<GetProductByIdQuery, Result<ProductDto>>,
    IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetProductWithItemsAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<ProductDto>.Failure("Product not found.", 404);
        }

        var dto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto, "Product retrieved successfully.", 200);
    }

    public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var queryable = _unitOfWork.Products.GetProductsQueryable().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.Trim();
            queryable = queryable.Where(p => p.ProductName.Contains(searchTerm));
        }

        queryable = request.Sort?.Trim().ToLower() switch
        {
            "name" => queryable.OrderBy(p => p.ProductName),
            "name_desc" => queryable.OrderByDescending(p => p.ProductName),
            "created" => queryable.OrderBy(p => p.CreatedOn),
            "created_desc" => queryable.OrderByDescending(p => p.CreatedOn),
            _ => queryable.OrderByDescending(p => p.CreatedOn)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var mappedItems = _mapper.Map<List<ProductDto>>(items);
        var pagedList = new PagedList<ProductDto>(mappedItems, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedList<ProductDto>>.Success(pagedList, "Products retrieved successfully.", 200);
    }
}

