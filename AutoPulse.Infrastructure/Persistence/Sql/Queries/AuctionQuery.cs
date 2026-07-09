using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;
using AutoPulse.Application.Application.Common.Interfaces.Queries;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Infrastructure.Persistence.Sql.Queries
{
    public class AuctionQuery : IAuctionQueries
    {
        private readonly AutoPulseDbContext _context;

        public AuctionQuery(AutoPulseDbContext context)
        {
            _context = context;
        }

        public async Task<AuctionDashboardDto?> GetAuctionDashboardAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            return await _context.Auctions
                                    // 1. Optimization: Turn off change tracker
                                    .AsNoTracking()
                                    // 2. Optimization: Prevent cartessian exaplosion
                                    .AsSplitQuery()
                                    .Where(a => a.Id == auctionId)
                                    // 3. Only select required columns
                                    .Select(a => new AuctionDashboardDto
                                    (
                                        auctionId,
                                        $"{a.Auctioneer!.UserName}'s {a.Vehicle!.Marquee} {a.Vehicle!.Model}" ?? string.Empty,
                                        a.StartingPrice!.Amount,
                                        a.StartingPrice.CurrencyCode,
                                        a.Bids.OrderByDescending(b => b.CreatedAt)
                                                .Select(b => new BidDto(
                                                    b.Id,
                                                    b.Bidder!.UserName ?? string.Empty,
                                                    b.Amount!.Amount,
                                                    b.Amount!.CurrencyCode,
                                                    b.CreatedAt.DateTime
                                                ))
                                                .ToList()
                                    ))
                                    .FirstOrDefaultAsync();
        }
    }
}
