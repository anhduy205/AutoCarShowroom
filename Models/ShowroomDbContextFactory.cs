using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoCarShowroom.Models
{
    public class ShowroomDbContextFactory : IDesignTimeDbContextFactory<ShowroomDbContext>
    {
        public ShowroomDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");

            var optionsBuilder = new DbContextOptionsBuilder<ShowroomDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ShowroomDbContext(optionsBuilder.Options);
        }
    }
}
