using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text;
using Tracker.Data;
using Tracker.Models;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Web;
using System.Security.Claims;
using static Tracker.Controllers.AuthController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private readonly IConfiguration _config;
        private readonly Guid _trackerGuid;
        private readonly ApplicationDbContext _context;
        private static readonly string _senderEmail = "relyq@relyq.dev";
        private SmtpClient _smtpClient = new SmtpClient
        {
            Host = "mail.relyq.dev",
            Port = 587,
            EnableSsl = false,
            Timeout = 30000
        };
        private Dictionary<string, string> _magicOrganizations;
        private Dictionary<string, string> _magicUsers;

        public UsersController(
            ApplicationUserManager userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IMapper mapper,
            IConfiguration config,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _config = config;
            _magicOrganizations = _config.GetSection("MagicOrganizations").Get<Dictionary<string, string>>();
            _magicUsers = _config.GetSection("MagicUsers").Get<Dictionary<string, string>>();
            _trackerGuid = new Guid(_magicOrganizations["TrackerOrganization"]);
            _context = context;

            _smtpClient.Credentials = new NetworkCredential(_senderEmail, _config["Secrets:SMTPPassword"]);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUser([FromQuery] GetUsersQueryObject query)
        {
            if (query.Limit < 0)
            {
                return BadRequest("Limit must be a positive integer");
            }

            if (query.Offset < 0)
            {
                return BadRequest("Offset must be a positive integer");
            }

            const int maxLimit = 50;

            // results limit
            if (query.Limit == 0 || query.Limit > maxLimit)
            {
                query.Limit = maxLimit;
            }

            (int, IEnumerable<UserDto>) usersDto;

            // logged in org
            var org = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (!query.All)
            {
                if (query.OrganizationId == null)
                {
                    return BadRequest("OrganizationId is required");
                }

                usersDto = await GetOrganizationUsers(query, org);
            }
            else
            {
                var user = await _userManager.FindByIdAsync(_authHelpers.GetUserId(User));

                // must be logged into tracker & be admin
                if (org.Id != _trackerGuid && !(await _userManager.IsInRoleAsync(user, "Administrator", await _context.Organization.FindAsync(_trackerGuid))))
                {
                    return Forbid();
                }

                usersDto = await GetAllUsers(query);
            }


            return Ok(new { count = usersDto.Item1, users = usersDto.Item2 });
        }

        private async Task<(int, IEnumerable<UserDto>)> GetAllUsers(GetUsersQueryObject query)
        {
            var rowsCount = await _userManager.Users
                .Where(u => query.Filter == null || (
                    EF.Functions.Like(u.NormalizedUserName, $"%{query.Filter.ToUpper()}%") ||
                    EF.Functions.Like(u.NormalizedEmail, $"%{query.Filter.ToUpper()}%") ||
                    (u.FirstName != null && EF.Functions.Like(u.FirstName, $"%{query.Filter.ToUpper()}%")) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName, $"%{query.Filter.ToUpper()}%"))))
                .CountAsync();

            var usersQuery = _userManager.Users
                .Where(u => query.Filter == null || (
                    EF.Functions.Like(u.NormalizedUserName, $"%{query.Filter.ToUpper()}%") ||
                    EF.Functions.Like(u.NormalizedEmail, $"%{query.Filter.ToUpper()}%") ||
                    (u.FirstName != null && EF.Functions.Like(u.FirstName, $"%{query.Filter.ToUpper()}%")) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName, $"%{query.Filter.ToUpper()}%"))));

            // get sort property & asc/desc
            // make sure sort parameter is [property].[direction]
            if (!string.IsNullOrWhiteSpace(query.Sort) && query.Sort.Split('.').Length == 2 && !string.IsNullOrWhiteSpace(query.Sort.Split('.')[1]))
            {
                var hsh = new Dictionary<string, IQueryable<ApplicationUser>>()
                {
                    {"created.desc",  usersQuery.Desc(t => t.Created)},
                    {"created.asc",  usersQuery.Asc(t => t.Created)},
                };

                if (hsh.ContainsKey(query.Sort))
                {
                    usersQuery = hsh[query.Sort];
                }
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(t => t.Created);
            }

            usersQuery = usersQuery
                .Skip(query.Offset)
                .Take(query.Limit);

            var users = await usersQuery.ToListAsync();

            var usersDto = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserDto>>(users);

            return (rowsCount, usersDto);
        }

        private async Task<(int, IEnumerable<UserDto>)> GetOrganizationUsers(GetUsersQueryObject query, Organization org)
        {
            var defaultOrg = await _context.Organization.FindAsync(new Guid(_magicOrganizations["DefaultOrganization"]));

            var rowsCount = await _userManager.Users
                .Where(u => u.Organizations.Contains(org) || u.Organizations.Contains(defaultOrg))
                .Where(u => query.Filter == null || (
                    EF.Functions.Like(u.NormalizedUserName, $"%{query.Filter.ToUpper()}%") ||
                    EF.Functions.Like(u.NormalizedEmail, $"%{query.Filter.ToUpper()}%") ||
                    (u.FirstName != null && EF.Functions.Like(u.FirstName, $"%{query.Filter.ToUpper()}%")) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName, $"%{query.Filter.ToUpper()}%"))))
                .CountAsync();

            var usersQuery = _userManager.Users
                .Include(u => u.Organizations)
                .Where(u => u.Organizations.Contains(org) || u.Organizations.Contains(defaultOrg))
                .Where(u => query.Filter == null || (
                    EF.Functions.Like(u.NormalizedUserName, $"%{query.Filter.ToUpper()}%") ||
                    EF.Functions.Like(u.NormalizedEmail, $"%{query.Filter.ToUpper()}%") ||
                    (u.FirstName != null && EF.Functions.Like(u.FirstName, $"%{query.Filter.ToUpper()}%")) ||
                    (u.LastName != null && EF.Functions.Like(u.LastName, $"%{query.Filter.ToUpper()}%"))));


            // get sort property & asc/desc
            // make sure sort parameter is [property].[direction]
            if (!string.IsNullOrWhiteSpace(query.Sort) && query.Sort.Split('.').Length == 2 && !string.IsNullOrWhiteSpace(query.Sort.Split('.')[1]))
            {
                var hsh = new Dictionary<string, IQueryable<ApplicationUser>>()
                {
                    {"created.desc",  usersQuery.Desc(t => t.Created)},
                    {"created.asc",  usersQuery.Asc(t => t.Created)},
                };

                if (hsh.ContainsKey(query.Sort))
                {
                    usersQuery = hsh[query.Sort];
                }
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(t => t.Created);
            }

            usersQuery = usersQuery
                .Skip(query.Offset)
                .Take(query.Limit);

            var users = await usersQuery.ToListAsync();

            var usersDto = _mapper.Map<IEnumerable<ApplicationUser>, IEnumerable<UserDto>>(users);

            return (rowsCount, usersDto);
        }

        // GET api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            var org = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (user == null)
            {
                return NotFound();
            }

            if (!user.Organizations.Contains(org) && org.Id != _trackerGuid && id != _magicUsers["DeletedUser"] && id != _magicUsers["UnassignedUser"])
            {
                return Forbid();
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        // GET api/Users/email/relyqx@gmail.com
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDto>> GetByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var org = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (user == null)
            {
                return NotFound();
            }

            if (!user.Organizations.Contains(org) && org.Id != _trackerGuid)
            {
                return Forbid();
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        public class NewUser
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string BaseUrl { get; set; }
        }


        // OrganizationId and Role are optional parameters, mainly used to add users to a new organization
        // if orgid is not provided, the new user will be added to the caller's org
        // if role is not provided, the new user will only have role "User"
        // if the user already exists it will be only added to the caller's organization
        // i should maybe have different endpoints to add user to org and to create user
        // POST api/Users
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> Post([FromBody] NewUser newUser)
        {
            if (string.IsNullOrEmpty(newUser.Email))
            {
                return BadRequest("Email can't be empty");
            }

            if (_authHelpers.GetUserOrganization(User) != _trackerGuid)
            {
                return Forbid();
            }

            // i think this returns null for non-existent users...
            var user = await _userManager.FindByEmailAsync(newUser.Email);

            if (user != null)
            {
                return BadRequest("User already exists");
            }

            user = CreateUser();

            await _userStore.SetUserNameAsync(user, newUser.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, newUser.Email, CancellationToken.None);
            IdentityResult result = await _userManager.CreateAsync(user, newUser.Password);
            if (result.Succeeded)
            {
                // send confirmation email

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                SendConfirmationEmail(newUser.BaseUrl, newUser.Email, code);

                // should try{} here as the user is already created and if this fails the caller might not know it
                var userDto = _mapper.Map<UserDto>(user);

                return Ok(userDto);
            }

            return StatusCode(500);
        }

        public class UserUpdate
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? PhoneNumber { get; set; }
        }

        // PUT api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UserUpdate userUpdate)
        {
            // request body should include all optional parameters
            // if parameter is not null then update it

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (_magicUsers.ContainsValue(user.Id) && user.Id != _magicUsers["RelyqUser"])
            {
                return Forbid();
            }

            // currently not possible to delete names or phone number

            if (!string.IsNullOrWhiteSpace(userUpdate.FirstName))
            {
                user.FirstName = userUpdate.FirstName;
            }
            
            if (!string.IsNullOrWhiteSpace(userUpdate.LastName))
            {
                user.LastName = userUpdate.LastName;
            }
            
            if (!string.IsNullOrWhiteSpace(userUpdate.PhoneNumber))
            {
                user.PhoneNumber = userUpdate.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // i might want to throw here instead
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        // DELETE api/Users/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            var org = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (org == null)
            {
                return BadRequest("Caller has no org");
            }

            // only authorize if the caller is from the same org as the user OR if the caller is a tracker admin
            // in addition to this, it's not possible to delete magic users
            if (org.Id != _trackerGuid || _magicUsers.ContainsValue(user.Id))
            {
                return Forbid();
            }

            // remove user from all org
            user.Organizations.Clear();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // i might want to throw here instead
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // sever all connections to the org
            var projects = await _context.Project
                .Where(p => p.AuthorId == id)
                .ToListAsync();

            var ticketsSubmitter = await _context.Ticket
                .Where(t => t.SubmitterId == id)
                .ToListAsync();

            var ticketsAssignee = await _context.Ticket
                .Where(t => t.AssigneeId == id)
                .ToListAsync();

            var comments = await _context.Comment
                .Where(c => c.AuthorId == id)
                .ToListAsync();

            // remove user-org-role entries
            var roles = await _context.Set<UserRole>().Where(ur => ur.UserId == user.Id).ToListAsync();
            roles.ForEach(r => _context.Set<UserRole>().Remove(r));

            projects.ForEach((p) => p.AuthorId = _magicUsers["DeletedUser"]);
            ticketsSubmitter.ForEach((t) => t.SubmitterId = _magicUsers["DeletedUser"]);
            ticketsAssignee.ForEach((t) => t.AssigneeId = _magicUsers["DeletedUser"]);
            comments.ForEach((c) => c.AuthorId = _magicUsers["DeletedUser"]);

            await _context.SaveChangesAsync();

            result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                // might want to throw instead
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        [HttpPost("passwordreset")]
        public async Task<IActionResult> PasswordReset(string baseUrl)
        {
            var user = await _userManager.FindByEmailAsync(User.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value);

            if (user.Id == _magicUsers["DeletedUser"] || user.Id == _magicUsers["UnassignedUser"] || user.Id == _magicUsers["DemoAdminUser"])
            {
                return Forbid();
            }

            var result = await _userManager.RemovePasswordAsync(user);

            if (result.Succeeded)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                SendPasswordResetEmail(baseUrl, user.Email, resetToken);

                return Ok();
            }

            result.Errors.ToList().ForEach(e =>
            {
                // do something
            });

            return BadRequest();
        }

        public class Test
        {
            public string UserId { get; set; }
            public string RoleId { get; set; }
            public string OrganizationId { get; set; }
        }

        // should return result
        private void SendConfirmationEmail(string baseUrl, string email, string confirmationToken)
        {
            var encodedToken = HttpUtility.UrlEncode(confirmationToken);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = "Tracker - Email confirmation",
                Body = $"Here's your email confirmation link: https://{baseUrl}/#/confirm/{email}/{encodedToken}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            _smtpClient.Send(mailMessage);
        }

        // should return result
        private void SendPasswordResetEmail(string baseUrl, string email, string resetToken)
        {
            var encodedToken = HttpUtility.UrlEncode(resetToken);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = "Tracker - Password reset",
                Body = $"Here's your password reset link: https://{baseUrl}/#/reset/{email}/{encodedToken}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            _smtpClient.Send(mailMessage);
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }

    public class GetUsersQueryObject
    {
        public bool All { get; set; } = false;
        public Guid? OrganizationId { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; } = 0;
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }
}
