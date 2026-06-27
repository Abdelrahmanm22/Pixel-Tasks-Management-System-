using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;

namespace Tasks.Repository.Data
{
    public class TaskContext : IdentityDbContext<AppUser>
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // DbSet Properties
        public DbSet<Corporation>      Corporations      { get; set; }
        public DbSet<Section>          Sections          { get; set; }
        public DbSet<TaskType>         TaskTypes         { get; set; }
        public DbSet<WorkTask>         WorkTasks         { get; set; }
        public DbSet<TaskAssignment>   TaskAssignments   { get; set; }
        public DbSet<TaskPoint>        TaskPoints        { get; set; }
        public DbSet<TaskPointStatus>  TaskPointStatuses { get; set; }
        public DbSet<TaskComment>      TaskComments      { get; set; }
    }
}
