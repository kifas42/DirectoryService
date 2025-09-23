using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public interface ILocationDbContext
{
    public DbSet<Location> Locations { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
