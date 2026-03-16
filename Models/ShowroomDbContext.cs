using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AutoCarShowroom.Models
{
    public class ShowroomDbContext : DbContext
    {
        public ShowroomDbContext(DbContextOptions<ShowroomDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
    }
}