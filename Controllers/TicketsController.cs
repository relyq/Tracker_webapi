#nullable disable
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
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq.Expressions;

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();
        private readonly IConfiguration _config;

        public TicketsController(ApplicationDbContext context, IMapper mapper, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicket([FromQuery] GetTicketsQueryObject query)
        {
            if (query.ProjectId == null)
            {
                return BadRequest("ProjectId can't be null");
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

            var project = await _context.Project.FindAsync(query.ProjectId);

            if (project == null)
            {
                // this exposes internals
                return NotFound("Project not found");
            }

            if (project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            TicketStatus status = null;

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                status = await _context.TicketStatus.FirstOrDefaultAsync(s => s.NormalizedStatus == query.Status.ToUpper());

                if (status == null)
                {
                    return NotFound($"Couldn't find status \"{query.Status}\"");
                }
            }

            var rowsCount = await _context.Ticket
                .Where(t => t.ProjectId == query.ProjectId)
                .Where(t => status == null || status == t.Status)
                .Where(t => query.Filter == null || (EF.Functions.Like(t.Title, $"%{query.Filter}%") || EF.Functions.Like(t.Description, $"%{query.Filter}%")))
                .CountAsync();

            var ticketsQuery = _context.Ticket
                .Include(t => t.Status)
                .Include(t => t.Type)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .Where(t => t.ProjectId == query.ProjectId)
                .Where(t => status == null || status == t.Status)
                .Where(t => query.Filter == null || (EF.Functions.Like(t.Title, $"%{query.Filter}%") || EF.Functions.Like(t.Description, $"%{query.Filter}%")));


            // get sort property & asc/desc
            if (!string.IsNullOrWhiteSpace(query.Sort) && !string.IsNullOrWhiteSpace(query.Sort.Split('.')[1]))
            {
                // this is the only way i found to do this strongly typed
                // forgive me father for what im about to do 
                // nigggg

                var hsh = new Dictionary<string, IQueryable<Ticket>>()
                {
                    {"id.desc", ticketsQuery.OrderByDescending(t => t.Id)},
                    {"id.asc",  ticketsQuery.OrderBy(t => t.Id)},
                    {"priority.desc",  ticketsQuery.Desc(t => t.Priority)},
                    {"priority.asc",  ticketsQuery.Asc(t => t.Priority)},
                    {"type.desc",  ticketsQuery.Desc(t => t.TicketTypeId)},
                    {"type.asc",  ticketsQuery.Asc(t => t.TicketTypeId)},
                    {"closed.desc",  ticketsQuery.Desc(t => t.Closed)},
                    {"closed.asc",  ticketsQuery.Asc(t => t.Closed)},
                    {"created.desc",  ticketsQuery.Desc(t => t.Created)},
                    {"created.asc",  ticketsQuery.Asc(t => t.Created)},
                };


                if (hsh.ContainsKey(query.Sort))
                {
                    ticketsQuery = hsh[query.Sort];
                }
            }
            else
            {
                ticketsQuery = ticketsQuery.OrderByDescending(t => t.Id);
            }

            ticketsQuery = ticketsQuery
                .Skip(query.Offset)
                .Take(query.Limit);

            var tickets = await ticketsQuery.ToListAsync();

            var ticketsDto = _mapper.Map<IEnumerable<Ticket>, IEnumerable<TicketDto>>(tickets);

            return Ok(new { count = rowsCount, tickets = ticketsDto });
        }





        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket = await _context.Ticket
                .Include(t => t.Status)
                .Include(t => t.Type)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            var ticketDto = _mapper.Map<TicketDto>(ticket);

            return ticketDto;
        }

        // redirects to comment controller until i decide it should be deleted
        // GET api/Tickets/5/Comments
        [HttpGet("{id}/Comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetTicketComments(int id)
        {
            var url = $"{Request.PathBase}/api/Comments?ticketid={id}";

            return Redirect(url);
        }

        // PUT: api/Tickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator,Developer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, TicketDto ticketDto)
        {
            if (id != ticketDto.Id)
            {
                return BadRequest("Path ID does not match object ID");
            }

            var p = await _context.Project.FindAsync(ticketDto.ProjectId);

            if (p == null)
            {
                // this exposes internals
                return NotFound("Project not found");
            }

            if (p.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(ticketDto.AssigneeId))
            {
                ticketDto.AssigneeId = _config["UnassignedUser"];
            }

            Ticket ticket = _mapper.Map<Ticket>(ticketDto);

            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
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

        // POST: api/Tickets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator,Developer")]
        [HttpPost]
        public async Task<ActionResult<TicketDto>> PostTicket(TicketDto ticketDto)
        {
            if (string.IsNullOrEmpty(ticketDto.AssigneeId))
            {
                ticketDto.AssigneeId = _config["UnassignedUser"];
            }

            var p = await _context.Project.FindAsync(ticketDto.ProjectId);

            if (p == null)
            {
                // this exposes internals
                return NotFound("Project not found");
            }

            if (p.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            ticketDto.Created = null;

            Ticket ticket = _mapper.Map<Ticket>(ticketDto);

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            ticket.SubmitterId = identity?.FindFirst("UserID")?.Value;

            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();

            ticketDto = _mapper.Map<TicketDto>(ticket);

            // this might not work properly
            return CreatedAtAction(nameof(GetTicket), new { id = ticketDto.Id }, ticketDto);
        }

        // DELETE: api/Tickets/5
        [Authorize(Roles = "Administrator,Developer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
    public class GetTicketsQueryObject
    {
        public int? ProjectId { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; } = 0;
        public string Status { get; set; }
        public string? Filter { get; set; }
        public string? Sort { get; set; }
    }

    public static class Extensions
    {
        public static IQueryable<Ticket> Desc(this IQueryable<Ticket> ticket, Expression<Func<Ticket, object>> keySelector)
        {
            return ticket.OrderByDescending(keySelector).ThenByDescending(t => t.Id);
        }

        public static IQueryable<Ticket> Asc(this IQueryable<Ticket> ticket, Expression<Func<Ticket, object>> keySelector)
        {
            return ticket.OrderBy(keySelector).ThenByDescending(t => t.Id);
        }
    }
}
