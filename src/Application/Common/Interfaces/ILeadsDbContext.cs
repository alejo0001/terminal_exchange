using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Domain.Leads.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CrmAPI.Application.Common.Interfaces;

public interface ILeadsDbContext
{
    DbSet<Lead> Leads { get; set; }

    DbSet<LeadEmail> LeadEmails { get; set; }

    DbSet<LeadPhone> LeadPhones { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    DatabaseFacade Database { get; }
}
 