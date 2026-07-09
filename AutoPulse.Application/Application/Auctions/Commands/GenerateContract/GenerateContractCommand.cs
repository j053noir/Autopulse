using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.GenerateContract
{
    public record GenerateContractCommand(
        Guid AuctionId,
        Guid WinnerId
    ) : IRequest<bool>;
}
