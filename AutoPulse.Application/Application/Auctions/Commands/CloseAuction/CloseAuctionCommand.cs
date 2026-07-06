using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CloseAuction
{
    public record CloseAuctionCommand(
        Guid AuctionId
        ) : IRequest<bool>;
}
