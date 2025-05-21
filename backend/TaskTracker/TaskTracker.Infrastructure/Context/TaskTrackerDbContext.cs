using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Entities;

namespace TaskTracker.Infrastructure.Context
{
    public class TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
