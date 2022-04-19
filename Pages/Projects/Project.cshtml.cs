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

namespace Tracker.Pages.Projects
{
    public class ProjectModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;

        public ProjectModel(Tracker.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Project Project { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project = await _context.Project
                .Include(p => p.Tickets).ThenInclude(t => t.Type)
                .Include(p => p.Tickets).ThenInclude(t => t.Status)
                .Include(p => p.Tickets).ThenInclude(t => t.Submitter)
                .Include(p => p.Tickets).ThenInclude(t => t.Assignee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Project == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
