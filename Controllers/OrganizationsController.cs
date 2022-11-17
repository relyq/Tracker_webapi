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
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private readonly Guid _trackerGuid;

        public OrganizationsController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration, ApplicationUserManager userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _trackerGuid = new Guid(_configuration["TrackerOrganization"]);
        }

        // GET: api/Organizations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganization()
        {
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
            {
                return Forbid();
            }

            var organizations = await _context.Organization.ToListAsync();

            var organizationsDto = _mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDto>>(organizations);

            return Ok(organizationsDto);
        }

        // GET: api/Organizations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganization(Guid id)
        {
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid && _authHelpers.GetUserOrganization(HttpContext.User) != id)
            {
                return Forbid();
            }

            var organization = await _context.Organization.FindAsync(id);

            if (organization == null)
            {
                return NotFound();
            }

            var organizationDto = _mapper.Map<OrganizationDto>(organization);

            return organizationDto;
        }

        // PUT: api/Organizations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrganization(Guid id, OrganizationDto organizationDto)
        {
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
            {
                return Forbid();
            }

            // these two should not be deleted
            if (id == new Guid(_configuration["DefaultOrganization"]) || id == new Guid(_configuration["TrackerOrganization"]))
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
        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> PostOrganization(OrganizationDto organizationDto)
        {
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
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

        // POST: api/Organizations/5/Users/5
        [Authorize(Roles = "Administrator")]
        [HttpPost("{organizationId}/Users/{userId}")]
        public async Task<ActionResult<UserDto>> AddUser(string organizationId, string userId, [FromBody] string? role = null)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return BadRequest("OrganizationId is required");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var user = await _userManager.FindByIdAsync(userId);

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
            if (user.Id == _configuration["DeletedUser"] || 
                user.Id == _configuration["RelyqUser"] || 
                user.Id == _configuration["UnassignedUser"])
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

            if (!string.IsNullOrEmpty(role))
            {
                if (!(await _roleManager.RoleExistsAsync(role)))
                {
                    // might instead want to keep going and record the error
                    return BadRequest("Role does not exist");
                }

                result = await _userManager.AddToRoleAsync(user, role, org);

                if (!result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
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

        // DELETE: api/Organizations/5/Users/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{organizationId}/Users/{userId}")]
        public async Task<IActionResult> RemoveUser(string organizationId, string userId)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return BadRequest("OrganizationId is required");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var user = await _userManager.FindByIdAsync(userId);

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
            if ((org != callerOrg && callerOrg.Id != _trackerGuid) ||
                (user.Id == _configuration["DeletedUser"] || user.Id == _configuration["RelyqUser"] || user.Id == _configuration["UnassignedUser"]))
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
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            var ticketsSubmitter = await _context.Ticket
                .Where(t => t.Project.Organization == org)
                .Where(t => t.SubmitterId == userId)
                .ToListAsync();

            var ticketsAssignee = await _context.Ticket
                .Where(t => t.Project.Organization == org)
                .Where(t => t.AssigneeId == userId)
                .ToListAsync();

            var comments = await _context.Comment
                .Where(c => c.Ticket.Project.Organization == org)
                .Where(c => c.AuthorId == userId)
                .ToListAsync();

            // remove user-org-role entries
            var orgRoles = await _context.Set<UserRole>().Where(ur => ur.UserId == user.Id && ur.OrganizationId == org.Id).ToListAsync();
            orgRoles.ForEach(r => _context.Set<UserRole>().Remove(r));

            projects.ForEach(p => p.AuthorId = _configuration["DeletedUser"]);
            ticketsSubmitter.ForEach(t => t.SubmitterId = _configuration["DeletedUser"]);
            ticketsAssignee.ForEach(t => t.AssigneeId = _configuration["DeletedUser"]);
            comments.ForEach(c => c.AuthorId = _configuration["DeletedUser"]);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Organizations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(Guid id)
        {
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
            {
                return Forbid();
            }

            // these two should not be deleted
            if (id == new Guid(_configuration["DefaultOrganization"]) || id == new Guid(_configuration["TrackerOrganization"]))
            {
                return StatusCode(418);
            }

            var organization = await _context.Organization.FindAsync(id);
            if (organization == null)
            {
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
}
