#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Data;
using Tracker.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Tracker.Pages.Tickets
{
    public class CreateModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(Tracker.Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            ProjectList = _context.Project.ToList();
            TicketTypeList = _context.TicketType.ToList();
            TicketStatusList = _context.TicketStatus.ToList();
            UserList = _context.Users.ToList();
            TicketVM = new TicketViewModel();
        }

        public SelectList ProjectSelectList { get; set; }
        public SelectList TicketTypeSelectList { get; set; }
        public SelectList TicketStatusSelectList { get; set; }
        public SelectList UserSelectList { get; set; }

        public List<Project> ProjectList { get; set; }
        public List<TicketType> TicketTypeList { get; set; }
        public List<TicketStatus> TicketStatusList { get; set; }
        public List<ApplicationUser> UserList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? projectid)
        {
            if(projectid == null)
            {
                return NotFound();
            }

            TicketVM.ProjectId = (int)projectid;

            ProjectSelectList = new SelectList(ProjectList, "Id", "Name");
            TicketTypeSelectList = new SelectList(TicketTypeList, "Id", "Type");
            // TicketStatusSelectList = new SelectList(TicketStatusList, "Id", "Status"); // ticket can only be created open for now
            UserSelectList = new SelectList(UserList, "Id", "UserName");

            return Page();
        }
        public class TicketViewModel
        {
            public int ProjectId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            [Display(Name = "Type")]
            public int TicketTypeId { get; set; }
            public int Priority { get; set; }
            [Display(Name = "Status")]
            public int TicketStatusId { get; set; }
            [Display(Name = "Assignee")]
            public string AssigneeId { get; set; }
        }

        public TicketViewModel TicketVM { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(TicketViewModel TicketVM)
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(TicketVM.ProjectId);
            }

            Ticket Ticket = new Ticket();

            Ticket.Title = TicketVM.Title;
            Ticket.Description = TicketVM.Description;
            Ticket.Priority = TicketVM.Priority;

            // if i got here ProjectId is supposed to not be null
            Ticket.ProjectId = TicketVM.ProjectId;
            Ticket.TicketTypeId = TicketVM.TicketTypeId;
            Ticket.TicketStatusId = TicketVM.TicketStatusId;

            var currentUserId = _userManager.GetUserId(User);
            Ticket.SubmitterId = currentUserId;
            Ticket.AssigneeId = TicketVM.AssigneeId;

            Ticket.Project = ProjectList.FirstOrDefault(p => p.Id == Ticket.ProjectId);
            Ticket.Type = TicketTypeList.FirstOrDefault(t => t.Id == Ticket.TicketTypeId);
            Ticket.Status = TicketStatusList.FirstOrDefault(s => s.Id == 1);

            Ticket.Submitter = UserList.FirstOrDefault(u => u.Id == Ticket.SubmitterId);
            Ticket.Assignee = UserList.FirstOrDefault(u => u.Id == Ticket.AssigneeId);

            Ticket.Created = DateTime.Now;

            if (!TryValidateModel(Ticket))
            {
                await OnGetAsync(TicketVM.ProjectId);
            }

            _context.Ticket.Add(Ticket);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Projects/Project", new { id = Ticket.ProjectId });
        }
    }
}
