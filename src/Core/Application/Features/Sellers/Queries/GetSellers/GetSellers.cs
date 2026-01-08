using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Application.DTOs;
using Domain.Common;

namespace Application.Features.Sellers.Queries.GetSellers;

public record GetSellersQuery(
    string? Search,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Region,
    int Page = 1,
    int PageSize = 10) : IQuery;

public class GetSellersHandler(ISellerReadRepository repository) : IQueryHandler<GetSellersQuery, PagedList<SellerDto>>
{
    public async Task<Result<PagedList<SellerDto>>> Handle(GetSellersQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 10;

        var searchSellers = await repository.SearchSellersAsync(query.Search,
            query.FirstName, query.LastName, query.Email, query.Region,
            page, pageSize, cancellationToken);

        return Result<PagedList<SellerDto>>.Success(searchSellers);
    }
}