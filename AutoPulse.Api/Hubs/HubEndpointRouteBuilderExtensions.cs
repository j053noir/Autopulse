namespace AutoPulse.Api.Hubs
{
    public static class HubEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Add endpoints for AutoPulse Real-Time Hubs.
        /// </summary>
        /// <param name="endpoints">IEndpointRouteBuilder instance.</param>
        /// <returns>IEndpointRouteBuilder instance.</returns>
        public static IEndpointRouteBuilder MapHubs(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints, nameof(endpoints));

            endpoints.MapHub<AuctionHub>("/hubs/auctions");

            return endpoints;
        }
    }
}
