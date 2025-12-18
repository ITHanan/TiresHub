using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Example: Explicitly telling EF Core the one-to-many relationship
            modelBuilder.Entity<TodoItem>()
                .HasOne(t => t.User)           // A TodoItem has one User
                .WithMany(u => u.ToDoList)     // A User has many TodoItems (the ToDoList collection)
                .HasForeignKey(t => t.UserId); // The UserId property is the foreign key
        }
    }
}
