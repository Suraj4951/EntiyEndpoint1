using Microsoft.EntityFrameworkCore;

namespace EntityEndpoint.API.Models
{
    public class EntityContext : DbContext
    {
        public EntityContext(DbContextOptions<EntityContext> options) : base(options)
        {
            
        }

        public DbSet<Entity> Entity { get; set; }   
    }


}

