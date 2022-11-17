using System.Security.Claims;

namespace Tracker.Controllers
{
    public class AuthHelpers
    {
        // returns logged in organization
        public Guid GetUserOrganization(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            return new Guid(identity?.FindFirst("OrganizationID")?.Value);
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;

            return identity?.FindFirst("UserID").Value;
        }
    }
}
