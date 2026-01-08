using Domain.Common;

namespace Domain.Entities.Sellers;

public static class SellerErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Seller.NotFound",
        description: $"The seller with ID '{id}' was not found.");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        code: "Seller.EmailAlreadyExists",
        description: $"Seller with email '{email}' already exists."
    );

    public static Error ErrorProcessingFile(string message) => Error.Unexpected(
        code: "Seller.ErrorProcessingFile",
        description: $"Error processing file: {message}"
    );

    public static Error Required(string propertyName) => Error.Validation(
        code: $"Seller.{propertyName}.Required",
        description: $"{propertyName} is required."
    );
}