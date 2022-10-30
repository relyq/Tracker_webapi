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

        /* no point in getting all tickets
        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicket()
        {
            var tickets = await _context.Ticket
                .Include(t => t.Status)
                .Include(t => t.Type)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .ToListAsync();

            var ticketsDto = _mapper.Map<IEnumerable<Ticket>, IEnumerable<TicketDto>>(tickets);

            return Ok(ticketsDto);
        }
        */

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

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            var ticketDto = _mapper.Map<TicketDto>(ticket);

            return ticketDto;
        }

        // GET api/Tickets/5/Comments
        [HttpGet("{id}/Comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetTicketComments(int id)
        {
            var ticket = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            IEnumerable<Comment> comments = await _context.Comment
                .Where(c => c.TicketId == id)
                .ToListAsync();

            IEnumerable<CommentDto> commentsDto = _mapper.Map<IEnumerable<Comment>, IEnumerable<CommentDto>>(comments);

            return Ok(commentsDto);
        }

        // PUT: api/Tickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator,Developer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, TicketDto ticketDto)
        {
            if (id != ticketDto.Id)
            {
                return BadRequest();
            }

            var p = await _context.Project.FindAsync(ticketDto.ProjectId);

            if (p != null && p.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(ticketDto.AssigneeId))
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

            if (p != null && p.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
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

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
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
}
