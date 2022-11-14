using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, IConfiguration config, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _config = config;
            _context = context;
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

            if (user.Id == _config["DeletedUser"])
            {
                return Forbid();
            }

            // this is wrong lmao
            var res = await _signinManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

            if (res.Succeeded)
            {
                var exp = 30;
                var token = GenerateJWT(user, exp);
                return Ok($"{{\"jwt\":\"{token}\"}}");
            }

            return NotFound();
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

        private string GenerateJWT(ApplicationUser user, int exp)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = GetClaims(user).Result;

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(exp),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var organization = await _context.Organization.FindAsync(user.OrganizationId);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.Id),
                new Claim("OrganizationID", user.OrganizationId.ToString()),
                new Claim("Organization", organization.Name)
            };

            (roles as List<string>).ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));



            return claims;
        }
    }
}
