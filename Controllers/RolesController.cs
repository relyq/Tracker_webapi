using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tracker.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class RoleDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        // GET: api/<RolesController>
        [HttpGet]
        public async Task<IEnumerable<RoleDto>> Get()
        {
            var roles = await _context.Roles.ToListAsync();

            List<RoleDto> rolesDto = new List<RoleDto>();
            
            roles.ForEach((role) => { rolesDto.Add(new RoleDto { Id = role.Id, Name = role.Name }); });

            return rolesDto;
        }

        // GET api/<RolesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<RolesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<RolesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RolesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
