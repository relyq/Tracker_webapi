using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Web;
using System.Linq.Expressions;

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();

        public ProjectsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // this might not be rest as it only gets projects from the user's organization
        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProject([FromQuery] GetProjectsQueryObject query)
        {
            if (query.Limit < 0)
            {
                return BadRequest("Limit must be a positive integer");
            }

            if (query.Offset < 0)
            {
                return BadRequest("Offset must be a positive integer");
            }

            if (_context.Project == null)
            {
                return NotFound();
            }

            const int maxLimit = 25;

            // results limit
            if (query.Limit == 0 || query.Limit > maxLimit)
            {
                query.Limit = maxLimit;
            }

            var organization = _authHelpers.GetUserOrganization(User);

            var rowsCount = await _context.Project
                .Where(p => p.OrganizationId == organization)
                .Where(p => query.Filter == null || (EF.Functions.Like(p.Name, $"%{query.Filter}%") || (p.Description != null && EF.Functions.Like(p.Description, $"%{query.Filter}%"))))
                .CountAsync();

            var projectsQuery = _context.Project
                .Where(p => p.OrganizationId == organization)
                .Where(p => query.Filter == null || (EF.Functions.Like(p.Name, $"%{query.Filter}%") || (p.Description != null && EF.Functions.Like(p.Description, $"%{query.Filter}%"))));


            // get sort property & asc/desc
            // make sure sort parameter is [property].[direction]
            if (!string.IsNullOrWhiteSpace(query.Sort) && query.Sort.Split('.').Length == 2 && !string.IsNullOrWhiteSpace(query.Sort.Split('.')[1]))
            {
                var hsh = new Dictionary<string, IQueryable<Project>>()
                {
                    {"id.desc", projectsQuery.OrderByDescending(t => t.Id)},
                    {"id.asc",  projectsQuery.OrderBy(t => t.Id)},
                    {"created.desc",  projectsQuery.Desc(t => t.Created)},
                    {"created.asc",  projectsQuery.Asc(t => t.Created)},
                };

                if (hsh.ContainsKey(query.Sort))
                {
                    projectsQuery = hsh[query.Sort];
                }
            }
            else
            {
                projectsQuery = projectsQuery.OrderByDescending(t => t.Id);
            }

            projectsQuery = projectsQuery
                .Skip(query.Offset)
                .Take(query.Limit);

            var projects = await projectsQuery.ToListAsync();


            var projectsDto = _mapper.Map<IEnumerable<Project>, IEnumerable<ProjectDto>>(projects);

            return Ok(new { count = rowsCount, projects = projectsDto });
        }

        // redirects to ticket controller until i decide it should be deleted
        // GET: api/Projects/5/Tickets
        [HttpGet("{id}/Tickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketByProject([FromRoute] int id, [FromQuery] GetTicketsQueryObject query)
        {
            var querystring = HttpUtility.ParseQueryString(Request.QueryString.ToString());
            querystring.Set("projectid", id.ToString());

            var url = $"{Request.PathBase}/api/Tickets?{querystring}";

            return Redirect(url);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            if (_context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            ProjectDto projectDto = _mapper.Map<ProjectDto>(project);

            return projectDto;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectDto projectDto)
        {
            if (id == null)
            {
                return BadRequest("Id can't be null");
            }

            if (id != projectDto.Id)
            {
                return BadRequest("Id does not match object Id");
            }

            Project project = await _context.Project
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null && project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            project = _mapper.Map<Project>(projectDto);

            project.OrganizationId = _authHelpers.GetUserOrganization(HttpContext.User);

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Projects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> PostProject(ProjectDto projectDto)

        {
            projectDto.Created = null;

            Project project = _mapper.Map<Project>(projectDto);

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            project.AuthorId = identity?.FindFirst("UserID")?.Value;

            project.OrganizationId = _authHelpers.GetUserOrganization(User);

            _context.Project.Add(project);
            await _context.SaveChangesAsync();

            projectDto = _mapper.Map<ProjectDto>(project);

            return CreatedAtAction("GetProject", new { id = projectDto.Id }, projectDto);
        }

        /* not working
        // PATCH: api/Projects/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> JsonPatchWithModelState(int id,
    [FromBody] JsonPatchDocument<ProjectDto> patchDoc)
        {
            if (patchDoc != null)
            {
                if (_context.Project == null)
                {
                    return NotFound();
                }

                var project = await _context.Project.FindAsync(id);

                if (project == null)
                {
                    return NotFound();
                }

                ProjectDto projectDto = _mapper.Map<ProjectDto>(project);

                patchDoc.ApplyTo(projectDto, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return new ObjectResult(projectDto);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        */

        // DELETE: api/Projects/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            if (_context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }

    public class GetProjectsQueryObject
    {
        public int Limit { get; set; }
        public int Offset { get; set; } = 0;
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }
}
