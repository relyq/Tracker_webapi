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
using AutoMapper;

namespace Tracker.Pages.Projects
{
    public class EditModel : PageModel
    {
        private readonly Tracker.Data.ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EditModel(Tracker.Data.ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [BindProperty]
        public ProjectDto ProjectDto { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProjectDto = _mapper.Map<ProjectDto>(await _context.Project.FirstOrDefaultAsync(m => m.Id == id));

            if (ProjectDto == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(ProjectDto projectDto)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Project Project = await _context.Project.FindAsync(projectDto.Id);

            if(Project == null)
            {
                return NotFound();
            }

            Project.Name = projectDto.Name;
            Project.Description = projectDto.Description;

            _context.Attach(Project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(Project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
