using CrmAPI.Application.Common.Interfaces;
using Domain.Tlmk.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Persistence;

public class TlmkDbContext : DbContext, ITlmkDbContext
{
    public TlmkDbContext(DbContextOptions<TlmkDbContext> options)
        : base(options)
    {
    }

    public DbSet<PedidoTlmk> PedidosTlmk { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoTlmk>()
            .ToTable("pedido_tlmk")
            .HasKey(x => x.NumPedido);
    }

    
}