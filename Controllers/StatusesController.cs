using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private Dictionary<string, string> _magicOrganizations;

        public StatusesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _magicOrganizations = _config.GetSection("MagicOrganizations").Get<Dictionary<string, string>>();
        }

        // GET: api/<StatusesController>
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var tstatuses = await _context.TicketStatus.Where(s => s.OrganizationId == new Guid(_magicOrganizations["DefaultOrganization"])).ToListAsync();

            IList<string> statuses = new List<string>();

            tstatuses.ForEach(s =>
            {
                statuses.Add(s.Status);
            });

            return statuses;
        }

        // GET api/<StatusesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<StatusesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<StatusesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<StatusesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
