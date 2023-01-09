using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private readonly Guid _trackerGuid;
        private Dictionary<string, string> _magicOrganizations;
        private Dictionary<string, string> _magicUsers;

        public OrganizationsController(ApplicationDbContext context, IMapper mapper, IConfiguration config, ApplicationUserManager userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _config = config;
            _magicOrganizations = _config.GetSection("MagicOrganizations").Get<Dictionary<string, string>>();
            _magicUsers = _config.GetSection("MagicUsers").Get<Dictionary<string, string>>();
            _trackerGuid = new Guid(_magicOrganizations["TrackerOrganization"]);
        }

        // GET: api/Organizations
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganization([FromQuery] GetOrganizationsQueryObject query)
        {
            if (_authHelpers.GetUserOrganization(User) != _trackerGuid)
            {
                return Forbid();
            }

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

            var rowsCount = await _context.Organization
                .Where(o => query.Filter == null || (EF.Functions.Like(o.Name, $"%{query.Filter}%")))
                .CountAsync();

            var organizationsQuery = _context.Organization
                .Where(o => query.Filter == null || (EF.Functions.Like(o.Name, $"%{query.Filter}%")));

            // get sort property & asc/desc
            // make sure sort parameter is [property].[direction]
            if (!string.IsNullOrWhiteSpace(query.Sort) && query.Sort.Split('.').Length == 2 && !string.IsNullOrWhiteSpace(query.Sort.Split('.')[1]))
            {
                var hsh = new Dictionary<string, IQueryable<Organization>>()
                {
                    {"created.desc",  organizationsQuery.Desc(t => t.Created)},
                    {"created.asc",  organizationsQuery.Asc(t => t.Created)},
                };

                if (hsh.ContainsKey(query.Sort))
                {
                    organizationsQuery = hsh[query.Sort];
                }
            }
            else
            {
                organizationsQuery = organizationsQuery.OrderByDescending(t => t.Created);
            }

            organizationsQuery = organizationsQuery
                .Skip(query.Offset)
                .Take(query.Limit);

            var organizations = await organizationsQuery.ToListAsync();

            var organizationsDto = _mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDto>>(organizations);

            return Ok(new { count = rowsCount, organizations = organizationsDto });
        }

        // GET: api/Organizations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganization(Guid id)
        {
            if (_authHelpers.GetUserOrganization(User) != _trackerGuid && _authHelpers.GetUserOrganization(User) != id)
            {
                return Forbid();
            }

            var organization = await _context.Organization.FindAsync(id);

            if (organization == null)
            {
                // this exposes internals
                return NotFound();
            }

            var organizationDto = _mapper.Map<OrganizationDto>(organization);

            return organizationDto;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganization(Guid id, OrganizationDto organizationDto)
        {
            if (_authHelpers.GetUserOrganization(User) != _trackerGuid)
            {
                return Forbid();
            }

            // these two should not be deleted
            if (id == new Guid(_magicOrganizations["DefaultOrganization"]) || id == new Guid(_magicOrganizations["TrackerOrganization"]))
            {
                return StatusCode(418);
            }

            var organization = _mapper.Map<Organization>(organizationDto);

            if (id != organization.Id)
            {
                return BadRequest();
            }

            _context.Entry(organization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Organizations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> PostOrganization(OrganizationDto organizationDto)
        {
            if (_authHelpers.GetUserOrganization(User) != _trackerGuid)
            {
                return Forbid();
            }

            organizationDto.Created = null;

            var organization = _mapper.Map<Organization>(organizationDto);

            _context.Organization.Add(organization);
            await _context.SaveChangesAsync();

            var id = organization.Id;

            organizationDto = _mapper.Map<OrganizationDto>(organization);

            return CreatedAtAction("GetOrganization", new { id = organizationDto.Id }, organizationDto);
        }

        public class UserOrgRole
        {
            public string? OrganizationId { get; set; }
            public string? UserEmail { get; set; }
            public string? Role { get; set; }
        }

        // POST: api/Organizations/5/Users/email@email.com
        [Authorize(Roles = "Administrator")]
        [HttpPost("{organizationId}/Users/{userEmail}")]
        public async Task<ActionResult<UserDto>> AddUser(string organizationId, string userEmail, [FromBody] UserOrgRole? userOrgRole = null)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return BadRequest("OrganizationId is required");
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("UserEmail is required");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var callerOrg = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (callerOrg == null)
            {
                return BadRequest("Caller has no organization");
            }

            var org = await _context.Organization.FindAsync(new Guid(organizationId));

            if (org == null)
            {
                return NotFound("Organization not found");
            }

            // it's not possible to add magic users to orgs
            if (user.Id == _magicUsers["DeletedUser"] ||
                user.Id == _magicUsers["UnassignedUser"])
            {
                return Forbid();
            }

            if (user.Organizations.Contains(org))
            {
                return BadRequest("User is already in this organization");
            }

            // i should send an email confirmation to the user to agree to being added to this org

            user.Organizations.Add(org);

            var result = await _userManager.AddToRoleAsync(user, "User", org);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (!string.IsNullOrEmpty(userOrgRole.Role))
            {
                if (!(await _roleManager.RoleExistsAsync(userOrgRole.Role)))
                {
                    // might instead want to keep going and record the error
                    return BadRequest("Role does not exist");
                }

                result = await _userManager.AddToRoleAsync(user, userOrgRole.Role, org);

                if (!result.Succeeded)
                {
                    if (!result.Errors.Any(err => err.Code == nameof(IdentityErrorDescriber.UserAlreadyInRole)))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            user = await _userManager.FindByIdAsync(user.Id);

            var userDto = _mapper.Map<UserDto>(user);

            ((List<Guid>)userDto.OrganizationsId).RemoveAll(o => !o.Equals(org.Id));
            ((List<OrganizationRole>)userDto.Roles).RemoveAll(r => !r.OrganizationId.Equals(org.Id));

            return Ok(userDto);
        }

        // DELETE: api/Organizations/5/Users/email@email.com
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{organizationId}/Users/{userEmail}")]
        public async Task<IActionResult> RemoveUser(string organizationId, string userEmail)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return BadRequest("OrganizationId is required");
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("UserEmail is required");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var callerOrg = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(User));

            if (callerOrg == null)
            {
                return BadRequest("Caller has no organization");
            }

            Organization org = await _context.Organization.FindAsync(new Guid(organizationId));

            if (org == null)
            {
                return NotFound("Organization not found");
            }

            // only authorize if the caller is from the same org as the user OR if the caller is a tracker admin
            // in addition to this, it's not possible to remove magic users from orgs
            if ((org != callerOrg && callerOrg.Id != _trackerGuid) || _magicUsers.ContainsValue(user.Id))
            {
                return Forbid();
            }

            if (!user.Organizations.Contains(org))
            {
                return BadRequest("User is not in this organization");
            }

            user.Organizations.Remove(org);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // sever all connections to the org
            var projects = await _context.Project
                .Where(p => p.Organization == org)
                .Where(p => p.AuthorId == user.Id)
                .ToListAsync();

            var ticketsSubmitter = await _context.Ticket
                .Where(t => t.Project.Organization == org)
                .Where(t => t.SubmitterId == user.Id)
                .ToListAsync();

            var ticketsAssignee = await _context.Ticket
                .Where(t => t.Project.Organization == org)
                .Where(t => t.AssigneeId == user.Id)
                .ToListAsync();

            var comments = await _context.Comment
                .Where(c => c.Ticket.Project.Organization == org)
                .Where(c => c.AuthorId == user.Id)
                .ToListAsync();

            // remove user-org-role entries
            var orgRoles = await _context.Set<UserRole>().Where(ur => ur.UserId == user.Id && ur.OrganizationId == org.Id).ToListAsync();
            orgRoles.ForEach(r => _context.Set<UserRole>().Remove(r));

            projects.ForEach(p => p.AuthorId = _magicUsers["DeletedUser"]);
            ticketsSubmitter.ForEach(t => t.SubmitterId = _magicUsers["DeletedUser"]);
            ticketsAssignee.ForEach(t => t.AssigneeId = _magicUsers["DeletedUser"]);
            comments.ForEach(c => c.AuthorId = _magicUsers["DeletedUser"]);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Organizations/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            if (_authHelpers.GetUserOrganization(User) != _trackerGuid)
            {
                return Forbid();
            }

            // these two should not be deleted
            if (id == new Guid(_magicOrganizations["DefaultOrganization"]) || id == new Guid(_magicOrganizations["TrackerOrganization"]))
            {
                return StatusCode(418);
            }

            var organization = await _context.Organization.FindAsync(id);
            if (organization == null)
            {
                // this exposes internals
                return NotFound();
            }

            _context.Organization.Remove(organization);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrganizationExists(Guid id)
        {
            return _context.Organization.Any(e => e.Id == id);
        }
    }

    public class GetOrganizationsQueryObject
    {
        public int Limit { get; set; }
        public int Offset { get; set; } = 0;
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }
}
