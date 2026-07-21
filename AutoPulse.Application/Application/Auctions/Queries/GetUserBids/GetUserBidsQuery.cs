using AutoPulse.Application.Application.Common.Interfaces;

namespace AutoPulse.Application.Application.Auctions.Queries.GetUserBids
{
    public record GetUserBidsQuery(Guid UserId) : IReadOnlyQuery<IReadOnlyList<UserBidDto>>;
}
