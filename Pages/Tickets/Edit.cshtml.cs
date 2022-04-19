#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;
using Tracker.Models;

namespace Tracker.Pages.Tickets
{
    public class EditModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;

        public EditModel(Tracker.Data.ApplicationDbContext context)
        {
            _context = context;
            ticketTypes = _context.TicketType.ToList();
            ticketStatuses = _context.TicketStatus.ToList();
            userList = _context.Users.ToList();
        }

        public Ticket Ticket { get; set; }
        public SelectList TicketTypesList { get; set; }
        public SelectList TicketStatusesList { get; set; }
        public SelectList UsersList { get; set; }

        public List<TicketType> ticketTypes { get; set; }
        public List<TicketStatus> ticketStatuses { get; set; }
        public List<ApplicationUser> userList { get; set; }

        public class TicketViewModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Priority { get; set; }
            public int TicketTypeId { get; set; }
            public int TicketStatusId { get; set; }
            public string AssigneeId { get; set; }
        }

        public TicketViewModel ViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket = await _context.Ticket.FirstOrDefaultAsync(m => m.Id == id);

            if (Ticket == null)
            {
                return NotFound();
            }

            ViewModel = new TicketViewModel();

            ViewModel.Title = Ticket.Title;
            ViewModel.Description = Ticket.Description;
            ViewModel.Priority = Ticket.Priority;
            ViewModel.TicketTypeId = Ticket.TicketTypeId;
            ViewModel.TicketStatusId = Ticket.TicketStatusId;
            ViewModel.AssigneeId = Ticket.AssigneeId;

            TicketTypesList = new SelectList(ticketTypes, "Id", "Type");
            TicketStatusesList = new SelectList(ticketStatuses, "Id", "Status");
            UsersList = new SelectList(userList, "Id", "UserName");

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(TicketViewModel ViewModel, int Id)
        {
            var Ticket = await _context.Ticket.FirstOrDefaultAsync(t => t.Id == Id);

            if (Ticket == null)
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                TicketTypesList = new SelectList(ticketTypes, "Id", "Type");
                TicketStatusesList = new SelectList(ticketStatuses, "Id", "Status");
                UsersList = new SelectList(userList, "Id", "UserName");

                return Page();
            }

            Ticket.Title = ViewModel.Title;
            Ticket.Description = ViewModel.Description;
            Ticket.Priority = ViewModel.Priority;
            Ticket.TicketTypeId = ViewModel.TicketTypeId;
            Ticket.TicketStatusId = ViewModel.TicketStatusId;
            Ticket.AssigneeId = ViewModel.AssigneeId;

            if(Ticket.TicketStatusId == 2 && Ticket.Closed == null) // 2 == Closed
            {
                Ticket.Closed = DateTime.Now;
            }

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

            return RedirectToPage("/Projects/Project", new { id = Ticket.ProjectId });
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
