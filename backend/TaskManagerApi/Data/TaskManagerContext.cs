using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data
{
    public class TaskManagerContext : DbContext
    {
        public TaskManagerContext(DbContextOptions<TaskManagerContext> options) : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Priority).HasConversion<int>();
            });

            // Seed some initial data
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = 1,
                    Title = "Complete project setup",
                    Description = "Set up the basic structure for the Task Manager application",
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CompletedAt = DateTime.UtcNow.AddDays(-1),
                    Priority = Priority.High
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Implement CRUD operations",
                    Description = "Add Create, Read, Update, Delete functionality for tasks",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Priority = Priority.Medium
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Design UI components",
                    Description = "Create React components for task management interface",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                    Priority = Priority.Medium
                }
            );
        }
    }
}