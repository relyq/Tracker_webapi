using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private readonly Guid _trackerGuid;

        public OrganizationsController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _trackerGuid = new Guid(_configuration["TrackerOrganization"]);
        }

        // GET: api/Organizations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganization()
        {
            if(_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
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
            if (_authHelpers.GetUserOrganization(HttpContext.User) != _trackerGuid)
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

            var organization = _mapper.Map<Organization>(organizationDto);

            _context.Organization.Add(organization);
            await _context.SaveChangesAsync();

            var id = organization.Id;

            organizationDto = _mapper.Map<OrganizationDto>(organization);

            return CreatedAtAction("GetOrganization", new { id = organizationDto.Id }, organizationDto);
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
