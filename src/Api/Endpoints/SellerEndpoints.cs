using Api.Extensions;
using Application.Abstractions.Messaging;
using Application.DTOs;
using Application.Features.Sellers.Commands.CreateSeller;
using Application.Features.Sellers.Commands.DeleteSeller;
using Application.Features.Sellers.Commands.UpdateSeller;
using Application.Features.Sellers.Commands.UploadSellers;
using Application.Features.Sellers.Queries.GetSellers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class SellerEndpoints
{
    public static void MapSellerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sellers")
            .WithTags("Sellers");

        // GET /api/sellers
        group.MapGet("/", async Task<Results<Ok<PagedList<SellerDto>>, ProblemHttpResult>> (
            [AsParameters] GetSellersQuery query,
            [FromServices] IQueryHandler<GetSellersQuery, PagedList<SellerDto>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Ok(result.Value);
        });

        // POST /api/sellers
        group.MapPost("/", async Task<Results<Created<Guid>, ProblemHttpResult>> (
            [FromBody] CreateSellerCommand request,
            [FromServices] ICommandHandler<CreateSellerCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Created($"/api/sellers/{result.Value}", result.Value);
        })
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // PUT /api/sellers/{id}
        group.MapPut("/{id:guid}", async Task<Results<NoContent, ProblemHttpResult>> (
            Guid id,
            [FromBody] UpdateSellerCommand request,
            [FromServices] ICommandHandler<UpdateSellerCommand> handler,
            CancellationToken ct) =>
        {
            var newRequest = request with { Id = id };
            var result = await handler.Handle(newRequest, ct);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.NoContent();
        })
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // DELETE /api/sellers/{id}
        group.MapDelete("/{id:guid}", async Task<Results<NoContent, ProblemHttpResult>> (
             [AsParameters] DeleteSellerCommand cmd,
             [FromServices] ICommandHandler<DeleteSellerCommand> handler,
             CancellationToken ct) =>
        {
            var result = await handler.Handle(cmd, ct);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.NoContent();
        })
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/sellers/upload
        group.MapPost("/upload", async Task<Results<Accepted, BadRequest<string>, ProblemHttpResult>> (
            IFormFile file,
            [FromServices] ICommandHandler<UploadSellersCommand> handler,
            CancellationToken ct) =>
        {
            if (file is null || file.Length == 0) return TypedResults.BadRequest("File is empty");

            using var stream = file.OpenReadStream();
            var command = new UploadSellersCommand(stream, file.FileName);

            var result = await handler.Handle(command, ct);

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }

            return TypedResults.Accepted("");
        })
        .DisableAntiforgery()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
