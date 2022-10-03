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
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();

        public CommentsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /* no point in getting all comments
        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComment()
        {
            return await _context.Comment.ToListAsync();
        }
        */

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment(int id)
        {
            var comment = await _context.Comment
                .Include(c => c.Ticket)
                .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            if ( comment.Ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            CommentDto commentDto = _mapper.Map<CommentDto>(comment);

            return commentDto;
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, CommentDto commentDto)
        {
            if (id != commentDto.Id)
            {
                return BadRequest();
            }

            var t = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == commentDto.TicketId);

            if (t != null && t.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            Comment comment = _mapper.Map<Comment>(commentDto);

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostComment(CommentDto commentDto)
        {
            var t = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == commentDto.TicketId);

            if (t != null && t.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            Comment comment = _mapper.Map<Comment>(commentDto);

            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = commentDto.Id }, commentDto);
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comment
                .Include(c => c.Ticket)
                .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.Ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.Id == id);
        }
    }
}
