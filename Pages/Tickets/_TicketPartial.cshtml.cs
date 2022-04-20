using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tracker.Models;

namespace Tracker.Pages.Tickets
{
    public class _TicketPartialModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;
        public _TicketPartialModel(Tracker.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }
    }
}
