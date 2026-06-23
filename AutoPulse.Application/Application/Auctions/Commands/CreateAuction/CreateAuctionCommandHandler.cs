using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Guid>
    {
        public CreateAuctionCommandHandler() { }

        public async Task<Guid> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            // 1. Business Rule Validations
            if (request.StartingPrice <= 0) throw new ArgumentException("StartingPrice must be greater than zero.");

            // 2. Persistance
            // TODO: Create Auction
            // TODO: Store in the database
            var auctionId = Guid.NewGuid();
            await Task.Delay(50, cancellationToken);            

            // 3. Return the new resource Id
            return auctionId;
        }
    }
}
