using Domain.Entities.Sellers;

namespace Domain.Abstractions.Repositories;

public interface ISellerRepository
{
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Seller>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task AddAsync(Seller seller, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seller seller, CancellationToken cancellationToken = default);
    Task DeleteAsync(Seller seller, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}