namespace Domain.Common;

public enum ErrorType
{
    None,
    NotFound,
    Validation,
    Conflict,
    Unexpected,
    Unauthorized
}

public record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error InvalidRecords = new("Error.UploadedFileInvalid", "Uploaded file contains no valid records.", ErrorType.Unexpected);

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static readonly Error NullValue = new("Error.NullValue", "A required value was null.", ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Unexpected(string code, string description) => new(code, description, ErrorType.Unexpected);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
}