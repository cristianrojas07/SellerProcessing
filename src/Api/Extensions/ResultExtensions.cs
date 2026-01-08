using Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Extensions;

public static class ResultExtensions
{
    public static ProblemHttpResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails.");
        }

        var error = result.Error;

        return TypedResults.Problem(
            statusCode: GetStatusCode(error.Type),
            title: error.Code,
            detail: error.Description,
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { error } }
            }
        );
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };
}