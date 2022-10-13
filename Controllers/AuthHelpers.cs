using System.Security.Claims;

namespace Tracker.Controllers
{
    public class AuthHelpers
    {
        public Guid GetUserOrganization(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            Guid organization = new Guid(identity?.FindFirst("OrganizationID")?.Value);

            return organization;
        }
    }
}
