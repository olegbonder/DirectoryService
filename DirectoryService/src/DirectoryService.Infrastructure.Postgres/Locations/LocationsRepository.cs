using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Infrastructure.Postgres.Locations
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken)
        {
            try
            {
                await _context.AddAsync(location, cancellationToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ex.InnerException?.Message ?? ex.Message;
            }

            return location.Id.Value;
        }
    }
}
