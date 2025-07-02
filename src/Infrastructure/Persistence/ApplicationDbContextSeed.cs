using System.Threading.Tasks;

namespace CrmAPI.Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    private ApplicationDbContext _context;

    public ApplicationDbContextSeed(ApplicationDbContext context) => _context = context;

    public async Task RunSeeders() { }
}
