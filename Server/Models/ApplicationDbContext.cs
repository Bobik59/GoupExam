using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    internal class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<CalorieGoal> CalorieGoals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CalorieIntakeEntry> CalorieIntakeEntries { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CalorieGoal>()
                .HasKey(c => c.GoalId);

            modelBuilder.Entity<CalorieIntakeEntry>()
                .HasKey(c => c.EntryId);

            // Уникальность названий продуктов и блюд
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Dish>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Связь между пользователями и их целями
            modelBuilder.Entity<CalorieGoal>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(cg => cg.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь между пользователями и их записями потребления калорий
            modelBuilder.Entity<CalorieIntakeEntry>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
