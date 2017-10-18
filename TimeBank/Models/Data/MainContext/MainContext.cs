using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBank.Models.Data.MainContext
{
    class MainContext : DbContext
    {
        public DbSet<WorkTime> WorkTimes { get; set; }

        public DbSet<Work> Works { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = System.IO.Path.Combine($"Data Source={Windows.Storage.ApplicationData.Current.LocalFolder.Path}", "main.db");
            optionsBuilder.UseSqlite(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkTime>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.DoneSeconds)
                .IsRequired();
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.WorkId)
                .IsRequired();
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.Year)
                .IsRequired();
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.Month)
                .IsRequired();
            modelBuilder.Entity<WorkTime>()
                .Property(x => x.Day)
                .IsRequired();

            modelBuilder.Entity<Work>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<Work>()
                .Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Work>()
                .Property(x => x.Name)
                .IsRequired();
        }
    }
}
