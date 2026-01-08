using Domain.Common;

namespace Domain.Entities.Sellers;

public sealed class Seller : Entity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; init; }

    private Seller() : base(Guid.NewGuid()) { }

    private Seller(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string region,
        bool isActive,
        DateTime createdAt) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Region = region;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public static Result<Seller> Create(string firstName, string lastName, string email, string phoneNumber, string region)
    {
        var validationResult = Validate(firstName, lastName, email, phoneNumber, region);

        if (validationResult.IsFailure) return Result<Seller>.Failure(validationResult.Error);

        var seller = new Seller(
            Guid.NewGuid(),
            firstName,
            lastName,
            email,
            phoneNumber,
            region,
            true,
            DateTime.UtcNow
        );

        return Result<Seller>.Success(seller);
    }

    public static Result<Seller> Import(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string region,
        bool isActive,
        DateTime createdAt)
    {
        var validation = Validate(firstName, lastName, email, phoneNumber, region);

        if (validation.IsFailure) return Result<Seller>.Failure(validation.Error);

        var seller = new Seller(
            id,
            firstName,
            lastName,
            email,
            phoneNumber,
            region,
            isActive,
            createdAt
        );

        return Result<Seller>.Success(seller);
    }

    public Result Update(string firstName, string lastName, string email, string phoneNumber, string region, bool isActive)
    {
        var validationResult = Validate(firstName, lastName, email, phoneNumber, region);
        var validateUpdate = ValidateUpdate(firstName, lastName, email, phoneNumber, region, isActive);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (validateUpdate.IsFailure)
        {
            return validateUpdate;
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Region = region;
        IsActive = isActive;

        return Result.Success();
    }

    private Result ValidateUpdate(string firstName, string lastName, string email, string phoneNumber, string region, bool isActive)
    {
        if (FirstName == firstName &&
            LastName == lastName &&
            PhoneNumber == phoneNumber &&
            Region == region &&
            IsActive == isActive)
        {
            return Result.Failure(Error.InvalidRecords);
        }

        return Result.Success();
    }

    private static Result Validate(string firstName, string lastName, string email, string phoneNumber, string region)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(SellerErrors.Required(nameof(FirstName)));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(SellerErrors.Required(nameof(LastName)));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(SellerErrors.Required(nameof(Email)));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure(SellerErrors.Required(nameof(PhoneNumber)));

        if (string.IsNullOrWhiteSpace(region))
            return Result.Failure(SellerErrors.Required(nameof(Region)));

        return Result.Success();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}