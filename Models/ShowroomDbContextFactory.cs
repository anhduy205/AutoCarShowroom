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
            var dataDirectory = Path.Combine(basePath, "App_Data");

            Directory.CreateDirectory(dataDirectory);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionStringTemplate = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection.");
            var connectionString = connectionStringTemplate.Replace("|DataDirectory|", dataDirectory, StringComparison.OrdinalIgnoreCase);

            var optionsBuilder = new DbContextOptionsBuilder<ShowroomDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new ShowroomDbContext(optionsBuilder.Options);
        }
    }
}
