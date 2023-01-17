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
using System.Net.Sockets;
using System.Security.Claims;

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserManager _userManager;
        private readonly IMapper _mapper;
        private readonly AuthHelpers _authHelpers = new AuthHelpers();

        public CommentsController(ApplicationDbContext context, ApplicationUserManager userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments([FromQuery] int ticketId)
        {
            if (ticketId == null)
            {
                return BadRequest("TicketId can't be null");
            }

            var ticket = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                // this exposes internals
                return NotFound("Ticket not found");
            }

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(User))
            {
                return Forbid();
            }

            IEnumerable<Comment> comments = await _context.Comment
                .OrderByDescending(c => c.Id)
                .Where(c => c.TicketId == ticketId)
                .ToListAsync();

            IEnumerable<CommentDto> commentsDto = _mapper.Map<IEnumerable<Comment>, IEnumerable<CommentDto>>(comments);

            return Ok(commentsDto);
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
        {
            var comment = await _context.Comment
                .Include(c => c.Ticket)
                .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            if (comment.Ticket == null)
            {
                // this exposes internals
                return NotFound("Ticket not found");
            }

            if (comment.Ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(User))
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

            if (t == null)
            {
                // this exposes internals
                return NotFound("Ticket not found");
            }

            if (t.Project.OrganizationId != _authHelpers.GetUserOrganization(User) || commentDto.AuthorId != _authHelpers.GetUserId(User))
            {
                return Forbid();
            }

            Comment comment = _mapper.Map<Comment>(commentDto);

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                t.Activity = DateTime.UtcNow;
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
            var ticket = await _context.Ticket
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == commentDto.TicketId);

            if (ticket == null)
            {
                // this exposes internals 
                return NotFound("Ticket does not exist");
            }

            if (ticket.Project.OrganizationId != _authHelpers.GetUserOrganization(HttpContext.User))
            {
                return Forbid();
            }

            commentDto.Created = null;

            Comment comment = _mapper.Map<Comment>(commentDto);

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            comment.AuthorId = identity?.FindFirst("UserID")?.Value;

            _context.Comment.Add(comment);
            ticket.Activity = DateTime.UtcNow;
            await _context.SaveChangesAsync();


            commentDto = _mapper.Map<CommentDto>(comment);

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

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == _authHelpers.GetUserId(User));
            var organization = await _context.Organization.FindAsync(_authHelpers.GetUserOrganization(HttpContext.User));

            if (comment.Ticket.Project.OrganizationId != organization.Id || (comment.AuthorId != user.Id && !await _userManager.IsInRoleAsync(user, "Administrator", organization)))
            {
                return Forbid();
            }

            comment.Ticket.Activity = DateTime.UtcNow;

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
