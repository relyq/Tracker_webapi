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
    public class TicketModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;

        public TicketModel(Tracker.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Ticket Ticket { get; set; }
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket = await _context.Ticket
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Submitter)
                .Include(t => t.Comments)
                .Include(t => t.Type)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Ticket == null)
            {
                return NotFound();
            }
            return Page();
        }

        // edit
        public async Task<IActionResult> OnPostAsync(int? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }

            Ticket = await _context.Ticket
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .Include(t => t.Submitter)
                .Include(t => t.Type)
                .FirstOrDefaultAsync(m => m.Id == Id);

            if(Ticket == null)
            {
                return NotFound();
            }
            if(Ticket.TicketStatusId == 2)
            {
                return Page();
            }

            Ticket.TicketStatusId = 2;
            Ticket.Closed = DateTime.Now;

            _context.Attach(Ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(Ticket.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return await OnGetAsync(Ticket.Id);
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
