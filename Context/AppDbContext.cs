using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ParallelEfContext.Model;

namespace ParallelEfContext.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Letter> Letters { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(ConnectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

        public static string ConnectionString => "Filename=db.sqlite";
    }
}
