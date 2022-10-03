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
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, IConfiguration config)
        {
            _userManager = userManager;
            _signinManager = signinManager;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(userLogin userLogin)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(userLogin.Email);

            // this is wrong lmao
            var res = await _signinManager.CheckPasswordSignInAsync(user, userLogin.Password, false);

            if (res.Succeeded)
            {
                var exp = 30;
                var token = GenerateJWT(user, exp);
                var expTime = (DateTime.UtcNow + new TimeSpan(0, exp, 0)).ToString("yyyy/MM/dd HH:mm:ss");
                return Ok($"{{\"jwt\":\"{token}\"}}");
            }

            return NotFound();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
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

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.Id),
                new Claim("OrganizationID", user.OrganizationId.ToString())
            };

            (roles as List<string>).ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));



            return claims;
        }
    }

    public class userLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
