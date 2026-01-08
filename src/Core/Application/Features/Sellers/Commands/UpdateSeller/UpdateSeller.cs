using Application.Abstractions.Messaging;
using Domain.Abstractions.Repositories;
using Domain.Common;
using Domain.Entities.Sellers;

namespace Application.Features.Sellers.Commands.UpdateSeller;

public record UpdateSellerCommand(Guid Id, string FirstName, string LastName, string Email, string PhoneNumber, string Region, bool IsActive) : ICommand;

public class UpdateSellerHandler(ISellerRepository repository) : ICommandHandler<UpdateSellerCommand>
{
    public async Task<Result> Handle(UpdateSellerCommand command, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (seller is null)
        {
            return Result.Failure(SellerErrors.NotFound(command.Id));
        }

        if (seller.Email != command.Email && await repository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            return Result.Failure(SellerErrors.EmailAlreadyExists(command.Email));
        }

        var result = seller.Update(command.FirstName, command.LastName, command.Email, command.PhoneNumber, command.Region, command.IsActive);

        if (result.IsFailure)
        {
            return result;
        }

        await repository.UpdateAsync(seller, cancellationToken);

        return Result.Success();
    }
}