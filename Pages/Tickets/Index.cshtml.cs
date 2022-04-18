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

namespace Tracker.Pages.Tickets
{
    public class IndexModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;

        public IndexModel(Tracker.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Ticket> Ticket { get;set; }

        public async Task OnGetAsync()
        {
            Ticket = await _context.Ticket
                .Include(t => t.Project)
                .Include(t => t.Type)
                .Include(t => t.Status)
                .Include(t => t.Submitter)
                .Include(t => t.Assignee)
                .ToListAsync();
        }
    }
}
