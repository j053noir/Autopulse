namespace AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto
{
    public record AuctionDashboardDto(
        Guid Id, 
        string Title, 
        decimal StartingPrice,
        string StartingCurrency,
        List<BidDto> HistoricBids
    );
}
