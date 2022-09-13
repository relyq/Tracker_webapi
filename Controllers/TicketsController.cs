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

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TicketsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var ticketDto = _mapper.Map<TicketDto>(ticket);

            return ticketDto;
        }

        // GET api/Tickets/5/Comments
        [HttpGet("{id}/Comments")]
        public async Task<IEnumerable<CommentDto>> GetTicketComments(int id)
        {
            IEnumerable<Comment> comments = await _context.Comment
                .Where(c => c.TicketId == id)
                .ToListAsync();

            IEnumerable<CommentDto> commentsDto = _mapper.Map<IEnumerable<Comment>, IEnumerable<CommentDto>>(comments);

            return commentsDto;
        }

        // PUT: api/Tickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, TicketDto ticketDto)
        {
            if (id != ticketDto.Id)
            {
                return BadRequest();
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
        [HttpPost]
        public async Task<ActionResult<TicketDto>> PostTicket(TicketDto ticketDto)
        {
            Ticket ticket = _mapper.Map<Ticket>(ticketDto);

            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();
            
            // this might not work properly
            return CreatedAtAction(nameof(GetTicket), new { id = ticketDto.Id }, ticketDto);
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
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
