using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace AutoPulse.Infrastructure.Authentication
{
    public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        private static readonly ConcurrentDictionary<string, AuthorizationPolicy> PolicyCache = new();

        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = _fallbackPolicyProvider.GetPolicyAsync(policyName).Result;
            if (policy is not null) return Task.FromResult<AuthorizationPolicy?>(policy);

            if (PolicyCache.TryGetValue(policyName, out var cachePolicy))
            {
                return Task.FromResult<AuthorizationPolicy?>(cachePolicy);
            }

            var newPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            PolicyCache.TryAdd(policyName, newPolicy);

            return Task.FromResult<AuthorizationPolicy?>(newPolicy);
        }
    }
}
