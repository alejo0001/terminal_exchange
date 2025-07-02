using Microsoft.EntityFrameworkCore;
using Domain.Tlmk.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface ITlmkDbContext
{
    public DbSet<PedidoTlmk> PedidosTlmk { get; set; }
}