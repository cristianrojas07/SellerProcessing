using Application.Abstractions.Messaging;
using Domain.Abstractions.Repositories;
using Domain.Common;
using Domain.Entities.Sellers;

namespace Application.Features.Sellers.Commands.DeleteSeller;

public record DeleteSellerCommand(Guid Id) : ICommand;

public class DeleteSellerHandler(ISellerRepository repository) : ICommandHandler<DeleteSellerCommand>
{
    public async Task<Result> Handle(DeleteSellerCommand command, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (seller is null)
        {
            return Result.Failure(SellerErrors.NotFound(command.Id));
        }

        await repository.DeleteAsync(seller, cancellationToken);

        return Result.Success();
    }
}