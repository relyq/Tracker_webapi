using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private Dictionary<string, string> _magicUsers;

        private readonly int _exp = 30;

        public AuthController(ApplicationUserManager userManager, SignInManager<ApplicationUser> signinManager, IConfiguration config, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _config = config;
            _context = context;
            _magicUsers = _config.GetSection("MagicUsers").Get<Dictionary<string, string>>();
        }

        public class UserLogin
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin userLogin)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(userLogin.Email);

            if (user == null)
            {
                return NotFound();
            }

            if (!user.Organizations.Any())
            {
                return BadRequest("User is in no organizations");
            }

            if (user.Id == _magicUsers["DeletedUser"] || user.Id == _magicUsers["UnassignedUser"])
            {
                return Forbid();
            }

            var res = await _signinManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

            if (!res.Succeeded)
            {
                return Unauthorized();
            }

            var jwt = await GenerateJWT(user, _exp);

            return Ok(new { jwt });
        }

        [AllowAnonymous]
        [HttpPost("login/demo")]
        public async Task<IActionResult> LoginDemo()
        {
            ApplicationUser user = await _userManager.FindByIdAsync(_magicUsers["DemoAdminUser"]);

            if (user == null)
            {
                return NotFound();
            }

            // test
            _authHelpers.DemoReset();

            var jwt = await GenerateJWT(user, _exp);

            return Ok(new { jwt });
        }

        [HttpPost("switchOrganization/{orgId}")]
        public async Task<IActionResult> SwitchOrganization(string orgId)
        {
            if (string.IsNullOrEmpty(orgId))
            {
                return BadRequest();
            }

            var org = await _context.Organization.FindAsync(new Guid(orgId));

            var user = await _userManager.FindByIdAsync(_authHelpers.GetUserId(User));

            if (!user.Organizations.Contains(org))
            {
                BadRequest();
            }

            var token = await GenerateJWT(user, _exp, orgId);

            return Ok($"{{\"jwt\":\"{token}\"}}");
        }

        public class EmailConfirmation
        {
            public string Email { get; set; }
            public string ConfirmationToken { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(EmailConfirmation emailConfirmation)
        {
            var user = await _userManager.FindByEmailAsync(emailConfirmation.Email);

            var result = await _userManager.ConfirmEmailAsync(user, emailConfirmation.ConfirmationToken);

            if (result.Succeeded)
            {
                return Ok();
            }

            return Unauthorized();
        }

        public class PasswordToken
        {
            public string Email { get; set; }
            public string ResetToken { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("passwordreset")]
        public async Task<IActionResult> PasswordReset(PasswordToken passwordReset)
        {
            var user = await _userManager.FindByEmailAsync(passwordReset.Email);

            var result = await _userManager.ResetPasswordAsync(user, passwordReset.ResetToken, passwordReset.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            result.Errors.ToList().ForEach(e =>
            {
                // do something
            });

            return BadRequest();
        }

        private async Task<string> GenerateJWT(ApplicationUser user, int exp, string? orgId = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = await GetClaims(user, orgId);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(exp),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<List<Claim>> GetClaims(ApplicationUser user, string orgId)
        {
            var organization = user.Organizations.LastOrDefault();
            if (!string.IsNullOrEmpty(orgId))
            {
                organization = user.Organizations.SingleOrDefault(o => o.Id == new Guid(orgId));
            }
            var roles = await _userManager.GetRolesInOrganizationAsync(user, organization);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.Id),
                new Claim("OrganizationID", organization.Id.ToString()),
                new Claim("Organization", organization.Name)
            };

            (roles as List<string>).ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

            return claims;
        }
    }
}
