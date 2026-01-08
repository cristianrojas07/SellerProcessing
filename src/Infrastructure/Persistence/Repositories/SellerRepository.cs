using Application.Abstractions.Repositories;
using Application.DTOs;
using Domain.Abstractions.Repositories;
using Domain.Entities.Sellers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SellerRepository(AppDbContext dbContext) : ISellerRepository, ISellerReadRepository
{
    public async Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers.FindAsync([id], cancellationToken);
    }

    public async Task<List<Seller>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Sellers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.FirstName.Contains(search) || s.LastName.Contains(search) || s.Email.Contains(search));
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        await dbContext.Sellers.AddAsync(seller, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        dbContext.Sellers.Update(seller);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Seller seller, CancellationToken cancellationToken = default)
    {
        dbContext.Sellers.Remove(seller);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<PagedList<SellerDto>> SearchSellersAsync(string? search, string? firstName, string? lastName, string? email, string? region, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Sellers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(region))
        {
            query = query.Where(x => x.Region == region);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(x =>
                x.FirstName.Contains(s) ||
                x.LastName.Contains(s) ||
                x.Email.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(x => x.FirstName.Contains(firstName.Trim()));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(x => x.LastName.Contains(lastName.Trim()));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(x => x.Email.Contains(email.Trim()));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SellerDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email,
                s.PhoneNumber,
                s.Region,
                s.IsActive,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedList<SellerDto>(items, totalCount);
    }
}