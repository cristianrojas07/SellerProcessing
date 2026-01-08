namespace Domain.Events;

public record SellerCreatedEvent(Guid Id, string FirstName, string LastName, string Email, string PhoneNumber, string Region, bool IsActive, DateTime CreatedAt);