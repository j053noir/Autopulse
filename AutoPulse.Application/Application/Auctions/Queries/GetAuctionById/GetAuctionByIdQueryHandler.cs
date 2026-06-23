using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionById
{
    public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDto?>
    {
        public GetAuctionByIdQueryHandler() { }

        public async Task<AuctionDto?> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Query in the DB
            // TODO: retrieve data from database
            await Task.Delay(25, cancellationToken);            

            AuctionDto? auction = null;

            if (Random.Shared.NextDouble() < 0.5)
            {
                auction = new AuctionDto(
                    Id: request.Id,
                    VehicleModel: "Toyonda Handibook",
                    CurrentPrice: 65000,
                    EndTime: DateTime.UtcNow.AddHours(4),
                    IsActive: true
                );
            }

            // Return retrieved data
            return auction;
        }
    }
}
