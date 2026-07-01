namespace AutoPulse.Domain.Common.Security
{
    public static class Permissions
    {
        public static class Auctions
        {
            public const string Create = "auctions:create";
            public const string Bid = "auctions:bid";
            public const string Delete = "auctions:delete";
        }

        public static class  Users
        {
            public const string Read = "users:read";
            public const string Write = "users:update";
        }
    }
}
