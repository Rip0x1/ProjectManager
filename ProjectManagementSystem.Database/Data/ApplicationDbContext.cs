using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Database.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<ProjectManagementSystem.Database.Entities.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectManagementSystem.Database.Entities.Task>()
                .ToTable("Tasks");
            modelBuilder.Entity<ProjectUser>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<ProjectUser>()
                .HasIndex(x => new {x.ProjectId, x.UserId})
                .IsUnique();
            modelBuilder.Entity<ProjectUser>()
                .HasOne(x => x.Project)
                .WithMany(x => x.ProjectUsers)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();
            modelBuilder.Entity<ProjectManagementSystem.Database.Entities.Task>()
                .HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                .HasOne(x => x.Task).WithMany(x => x.Comments)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }




    }
}
