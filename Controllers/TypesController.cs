using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tracker.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tracker.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TypesController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TypesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Types
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var ttypes = await _context.TicketType.Where(t => t.OrganizationId == new Guid(_configuration["DefaultOrganization"])).ToListAsync();

            IList<string> types = new List<string>();

            ttypes.ForEach(t =>
            {
                types.Add(t.Type);
            });

            return types;
        }

        // GET api/<TicketTypesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<TicketTypesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<TicketTypesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TicketTypesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
