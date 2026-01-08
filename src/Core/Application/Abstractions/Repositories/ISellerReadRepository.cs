using Application.DTOs;

namespace Application.Abstractions.Repositories;

public interface ISellerReadRepository
{
    Task<PagedList<SellerDto>> SearchSellersAsync(
        string? search,
        string? firstName,
        string? lastName,
        string? email,
        string? region,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
