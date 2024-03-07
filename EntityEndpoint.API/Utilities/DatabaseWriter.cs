using EntityEndpoint.API.Models;
using System;
using System.Threading.Tasks;

namespace EntityEndpoint.API.Data
{
    public class DatabaseWriter
    {
        private readonly EntityContext _dbContext;

        public DatabaseWriter(EntityContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task WriteToDatabaseAsync(Action<EntityContext> databaseAction, int maxRetries = 3, TimeSpan delayBetweenRetries = default)
        {
            int retryCount = 0;
            bool success = false;

            while (!success && retryCount < maxRetries)
            {
                try
                {
                    databaseAction(_dbContext);
                    await _dbContext.SaveChangesAsync();
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Retry {retryCount + 1} failed: {ex.Message}");

                    retryCount++;

                    if (delayBetweenRetries != default)
                    {
                        await Task.Delay(delayBetweenRetries);
                    }
                }
            }

            if (!success)
            {
                throw new Exception($"Failed to write to database after {maxRetries} retries.");
            }
        }
    }
}
