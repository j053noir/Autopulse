using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.ReopenAuctionCompensation
{
    public record ReopenAuctionCompensationCommand(
        Guid CorrelationId,
        Guid AuctionId,
        string ErrorMessage
    ) : IRequest<bool>;
}
