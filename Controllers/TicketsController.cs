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

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
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

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicket()
        {
            return await _context.Ticket.ToListAsync();
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // GET api/Tickets/5/Comments
        [HttpGet("{id}/Comments")]
        public async Task<IEnumerable<CommentDto>> GetTicketComments(int id)
        {
            IEnumerable<Comment> CommentList = await _context.Comment
                .Where(c => c.TicketId == id)
                .ToListAsync();

            IEnumerable<CommentDto> CommentDtoList = _mapper.Map<IEnumerable<Comment>, IEnumerable<CommentDto>>(CommentList);

            foreach (var com in CommentDtoList)
            {
                com.AuthorUsername = _context.Users.Find(com.AuthorId).UserName;
            }

            return CommentDtoList;
        }

        // PUT: api/Tickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return BadRequest();
            }

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
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
        }

        // POST: api/Tickets/5/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{id}/Comments")]
        public async Task<IEnumerable<CommentDto>> PostTicketComment(int id, CommentDto commentDto)
        {
            Comment comment = new()
            {
                AuthorId = commentDto.AuthorId,
                TicketId = commentDto.TicketId,
                ParentId = commentDto.ParentId,
                Content = commentDto.Content,
                Created = commentDto.Created,
                Author = await _context.Users.FindAsync(commentDto.AuthorId),
                Ticket = await _context.Ticket.FindAsync(commentDto.TicketId),
                Parent = (commentDto.ParentId != null) ? await _context.Comment.FindAsync(commentDto.ParentId) : null,
            };

            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            IEnumerable<Comment> CommentList = await _context.Comment
                .Where(c => c.TicketId == id)
                .ToListAsync();

            IEnumerable<CommentDto> CommentDtoList = _mapper.Map<IEnumerable<Comment>, IEnumerable<CommentDto>>(CommentList);

            foreach(var com in CommentDtoList)
			{
                com.AuthorUsername = _context.Users.Find(com.AuthorId).UserName;
			}

            return CommentDtoList;
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
