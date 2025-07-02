using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Domain.Leads.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Persistence;

public class LeadsDbContext : DbContext, ILeadsDbContext
{

    public LeadsDbContext(DbContextOptions<LeadsDbContext> options)
        : base(options)
    {
    }
        
    public DbSet<Lead> Leads { get; set; }
    public DbSet<LeadEmail> LeadEmails { get; set; }
    public DbSet<LeadPhone> LeadPhones { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>()
            .ToTable("potenciales_unificada")
            .HasKey(x => x.id);
        modelBuilder.Entity<LeadEmail>()
            .ToTable("potenciales_unificada_emails")
            .HasKey(x => x.id);
        modelBuilder.Entity<LeadPhone>()
            .ToTable("potenciales_unificada_telefonos")
            .HasKey(x => x.id);
       
    }
}