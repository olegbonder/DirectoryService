using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Infrastructure.Permission
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => throw new NotImplementedException();
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => throw new NotImplementedException();
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) => throw new NotImplementedException();
    }
}