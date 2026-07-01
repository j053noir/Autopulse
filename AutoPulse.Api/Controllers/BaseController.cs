using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace AutoPulse.Api.Controllers
{
    public abstract class BaseController : Controller
    {
        protected Guid GetUserClaimId(string param)
        {
            var userClaimId = User.Claims.FirstOrDefault(c =>
                                                        c.Type == JwtRegisteredClaimNames.Sub ||
                                                        c.Type == "sub" ||
                                                        c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userClaimId == null) throw new ArgumentNullException($"Missing {param}");
            if (!Guid.TryParse(userClaimId, out Guid userId)) throw new ArgumentException($"{param} is not a valid Id");

            return userId;
        }
    }
}
