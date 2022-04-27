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
    public class TicketModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TicketModel(Tracker.Data.ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public TicketDto TicketDto { get; set; }
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TicketDto = _mapper.Map<TicketDto>(await _context.Ticket.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == id));

            TicketDto.Status = (await _context.TicketStatus.FindAsync(TicketDto.TicketStatusId)).Status;
            TicketDto.Type = (await _context.TicketType.FindAsync(TicketDto.TicketTypeId)).Type;
            TicketDto.SubmitterUsername = (await _context.Users.FindAsync(TicketDto.SubmitterId)).UserName;
            TicketDto.AssigneeUsername = (await _context.Users.FindAsync(TicketDto.AssigneeId)).UserName;

            if (TicketDto == null)
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

            TicketDto = _mapper.Map<TicketDto>(await _context.Ticket.FirstOrDefaultAsync(t => t.Id == Id));

            if(TicketDto == null)
            {
                return NotFound();
            }
            if(TicketDto.TicketStatusId == 2)
            {
                return Page();
            }

            // not needed
            TicketDto.TicketStatusId = 2;
            TicketDto.Closed = DateTime.Now;

            Ticket Ticket = new Ticket();

            // does this work? do i need to include childs?
            Ticket = await _context.Ticket.FindAsync(TicketDto.Id);

            Ticket.TicketStatusId = 2;
            Ticket.Closed = DateTime.Now;

            _context.Attach(Ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(TicketDto.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return await OnGetAsync(TicketDto.Id);
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
