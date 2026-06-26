using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        // DbSet Properties for each entity in your domain model here.
        public DbSet<Corporation> Corporations { get; set; }
    }
}
