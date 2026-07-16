namespace AutoPulse.Domain.Common.Security
{
    public static class Permissions
    {
        public static class Auctions
        {
            public const string Create = "auctions:create";
            public const string Bid = "auctions:bid";
            public const string Delete = "auctions:delete";
            public const string Read = "auctions:read";
            public const string Close = "auctions:close";
        }

        public static class  Users
        {
            public const string Read = "users:read";
            public const string Write = "users:update";
        }

        public static class Telemetry
        {
            public const string Process = "telemetry:process";
            public const string Benchmark = "telemetry:benchmark";
        }

        public static class Claims
        {
            public const string FamilyId = "family_id";
        }

        public static class CacheKeys
        {
            public const string RateLimitAuth = "ratelimit:auth";
            public const string RateLimitGeneral = "ratelimit:general";
        }

        public static class Policies
        {
            public const string AuthPolicy = "auth-policy";
            public const string ApiGeneralPolicy = "api-general-policy";
        }
    }
}
