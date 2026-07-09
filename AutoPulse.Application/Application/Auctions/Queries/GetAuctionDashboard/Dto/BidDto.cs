namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto
{
    public record BidDto(
        Guid Id, 
        string BidderName, 
        decimal Amount,
        string CurrencyCode,
        DateTime CreatedAt
    );
}
