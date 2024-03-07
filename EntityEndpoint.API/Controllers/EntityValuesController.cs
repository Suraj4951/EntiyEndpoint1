using EntityEndpoint.API.Data;
using EntityEndpoint.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityEndpoint.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityValuesController : ControllerBase
    {
        private readonly EntityContext _dbContext;

        private readonly DatabaseWriter _databaseWriter;

        public EntityValuesController(EntityContext dbContext, DatabaseWriter databaseWriter)
        {
            _dbContext = dbContext;
            _databaseWriter = databaseWriter;
        }

        //Get All Etities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Entity>>> GetEntities(
     [FromQuery] int pageNumber = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] string orderBy = "Id",
     [FromQuery] bool ascending = true)
        {
            IQueryable<Entity> query = _dbContext.Entity
                .Include(e => e.Addresses)
                .Include(e => e.Dates)
                .Include(e => e.Names);

            // Sorting
            switch (orderBy.ToLower())
            {
                case "id":
                    query = ascending ? query.OrderBy(e => e.Id) : query.OrderByDescending(e => e.Id);
                    break;
                case "gender":
                    query = ascending ? query.OrderBy(e => e.Gender) : query.OrderByDescending(e => e.Gender);
                    break;
                default:
                    query = query.OrderBy(e => e.Id);
                    break;
            }

            // Pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var entities = await query.ToListAsync();

            if (entities == null || entities.Count == 0)
            {
                return NotFound();
            }

            return entities;
        }


        //GetById
        [HttpGet("{id}")]
        public async Task<ActionResult<Entity>> GetEntity(string id)
        {
            var entity = await _dbContext.Entity
                .Include(e => e.Addresses)
                .Include(e => e.Dates)
                .Include(e => e.Names)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return NotFound();
            }
            return entity;
        }

        /*Post Data
         *Using WriteToDatabaseAsync of DatabaseWriter for Implementing Retry and Backoff Mechanism
        */
        [HttpPost]
        public async Task<ActionResult<Entity>> PostEntity(Entity entity)
        {
            try
            {
                await _databaseWriter.WriteToDatabaseAsync(dbContext => dbContext.Entity.Add(entity));
                return CreatedAtAction(nameof(GetEntity), new { id = entity.Id }, entity);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Failed to add entity to the database.");
            }
        }

        //Update Data
        [HttpPut]
        public async Task<ActionResult> PutEntity(string id, Entity entity)
        {
            if (id != entity.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(entity).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityAvailable(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }

            }
            return Ok();
        }
        //DeleteById
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntity(string id)
        {
            var entity = await _dbContext.Entity.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _dbContext.Entity.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        //Searching
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Entity>>> SearchEntities([FromQuery] string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return BadRequest("Search parameter is required.");
            }

            var entities = await _dbContext.Entity
                .Include(e => e.Addresses)
                .Include(e => e.Dates)
                .Include(e => e.Names)
                .Where(e =>
                    e.Names.Any(n =>
                        n.FirstName.Contains(search) ||
                        n.MiddleName.Contains(search) ||
                        n.Surname.Contains(search)) ||
                    e.Addresses.Any(a =>
                        a.AddressLine.Contains(search) ||
                        a.City.Contains(search) ||
                        a.Country.Contains(search)))
                .ToListAsync();

            if (entities == null || entities.Count == 0)
            {
                return NotFound();
            }

            return entities;
        }

        //Filtering
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Entity>>> FilterEntities(
    [FromQuery] string gender,
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] string[] countries)
        {
            IQueryable<Entity> query = _dbContext.Entity
                .Include(e => e.Addresses)
                .Include(e => e.Dates)
                .Include(e => e.Names);

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(e => e.Gender == gender);
            }

            if (startDate != null && endDate != null)
            {
                endDate = endDate.Value.AddDays(1); // Make the end date inclusive
                query = query.Where(e => e.Dates.Any(d => d.DateTime >= startDate && d.DateTime < endDate));
            }

            if (countries != null && countries.Length > 0)
            {
                query = query.Where(e => e.Addresses.Any(a => countries.Contains(a.Country)));
            }

            var entities = await query.ToListAsync();

            if (entities == null || entities.Count == 0)
            {
                return NotFound();
            }

            return entities;
        }


        //Check Entity Available
        private bool EntityAvailable(string id)
        {
            return (_dbContext.Entity?.Any(x => x.Id == id)).GetValueOrDefault() == false;
        }
    }
}
