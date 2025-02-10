using GAEFT9_HSZF_2024251.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAEFT9_HSZF_2024251.Persistence.MsSql
{
    public class BreedingDbContext : DbContext
    {
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Pairing> Pairings { get; set; }

        public BreedingDbContext(DbContextOptions<BreedingDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Animal>()
                .HasMany(a => a.Children)
                .WithOne(c => c.Mother)
                .HasForeignKey(c => c.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<Animal>()
                .HasMany<Animal>()
                .WithOne(c => c.Father)
                .HasForeignKey(c => c.FatherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Mother)
                .WithMany()
                .HasForeignKey(p => p.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pairing>()
                .HasOne(p => p.Father)
                .WithMany()
                .HasForeignKey(p => p.FatherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
