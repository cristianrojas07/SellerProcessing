namespace Application.DTOs;

public record SellerDto(Guid Id, string FirstName, string LastName, string Email, string PhoneNumber, string Region, bool IsActive, DateTime CreatedAt);

public record PagedList<T>(List<T> Items, int TotalCount);

public record SellerImportDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}