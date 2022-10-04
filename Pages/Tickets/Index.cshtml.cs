#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;
using AutoMapper;

namespace Tracker.Pages.Tickets
{
    public class IndexModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public IndexModel(Tracker.Data.ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<TicketDto> TicketDto { get; set; }
        public int ProjectId { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectid)
        {
            if (projectid == null)
            {
                return NotFound();
            }

            ProjectId = (int)projectid;

            var tickets = await _context.Ticket
                .Where(t => t.ProjectId == ProjectId)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .ToListAsync();

            TicketDto = _mapper.Map<IEnumerable<TicketDto>>(tickets);

            return Page();
        }
    }
}