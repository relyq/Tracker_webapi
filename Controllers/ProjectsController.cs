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
using Microsoft.AspNetCore.JsonPatch;

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProject()
        {
          if (_context.Project == null)
          {
              return NotFound();
          }

            var projects = await _context.Project.ToListAsync();

            var projectsDto = _mapper.Map<IEnumerable<Project>, IEnumerable<ProjectDto>>(projects);

          return Ok(projectsDto);
        }

        // GET: api/Projects/5/Tickets
        [HttpGet("{id}/Tickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketByProject(int id)
        {
            var tickets = await _context.Ticket
                .Where(t => t.ProjectId == id)
                .Include(t => t.Status)
                .Include(t => t.Type)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .ToListAsync();

            var ticketsDto = _mapper.Map<IEnumerable<Ticket>, IEnumerable<TicketDto>>(tickets);

            return Ok(ticketsDto);
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

            ProjectDto projectDto = _mapper.Map<ProjectDto>(project);

            return projectDto;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectDto projectDto)
        {
            if (id != projectDto.Id)
            {
                return BadRequest();
            }

            Project project = _mapper.Map<Project>(projectDto);

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
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> PostProject(ProjectDto projectDto)
        {
            Project project = _mapper.Map<Project>(projectDto);

            _context.Project.Add(project);
            await _context.SaveChangesAsync();

            // this might not work properly
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

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
