using Microsoft.EntityFrameworkCore;
using TaskSystem.Models;

namespace TaskSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee>       Employees       { get; set; }
        public DbSet<Department>     Departments     { get; set; }
        public DbSet<Project>        Projects        { get; set; }
        public DbSet<ProjectEmployee> ProjectEmployees { get; set; }
        public DbSet<TaskItem>       Tasks           { get; set; }
        public DbSet<TaskAssignment> TaskAssignments  { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ProjectEmployee → composite PK
            modelBuilder.Entity<ProjectEmployee>()
                .HasKey(pe => new { pe.Proj_Id, pe.Emp_Id });

            // ProjectEmployee → Project
            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Project)
                .WithMany(p => p.ProjectEmployees)
                .HasForeignKey(pe => pe.Proj_Id);

            // ProjectEmployee → Employee
            modelBuilder.Entity<ProjectEmployee>()
                .HasOne(pe => pe.Employee)
                .WithMany(e => e.ProjectEmployees)
                .HasForeignKey(pe => pe.Emp_Id);

            // Employee → Department
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.Dept_Id)
                .OnDelete(DeleteBehavior.SetNull);

            // TaskItem → Project
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.Proj_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskAssignment → TaskItem
            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.TaskAssignments)
                .HasForeignKey(ta => ta.Task_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskAssignment → Employee
            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Employee)
                .WithMany(e => e.TaskAssignments)
                .HasForeignKey(ta => ta.Emp_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Store enums as strings in Oracle
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Task_Status)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Task_Priority)
                .HasConversion<string>();
        }
    }
}
