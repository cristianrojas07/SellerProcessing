using Application.Abstractions.Messaging;
using Domain.Abstractions.Repositories;
using Domain.Common;
using Domain.Entities.Sellers;

namespace Application.Features.Sellers.Commands.CreateSeller;

public record CreateSellerCommand(string FirstName, string LastName, string Email, string PhoneNumber, string Region) : ICommand;

public class CreateSellerHandler(ISellerRepository repository) : ICommandHandler<CreateSellerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSellerCommand command, CancellationToken cancellationToken)
    {
        if (await repository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            return Result<Guid>.Failure(SellerErrors.EmailAlreadyExists(command.Email));
        }

        var sellerResult = Seller.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Region);

        if (sellerResult.IsFailure)
        {
            return Result<Guid>.Failure(sellerResult.Error);
        }

        await repository.AddAsync(sellerResult.Value!, cancellationToken);

        return Result<Guid>.Success(sellerResult.Value!.Id);
    }
}