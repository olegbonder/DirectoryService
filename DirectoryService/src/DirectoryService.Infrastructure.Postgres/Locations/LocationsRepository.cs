using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Locations
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationsRepository> _logger;

        public LocationsRepository(ApplicationDbContext context, ILogger<LocationsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellationToken)
        {
            try
            {
                await _context.AddAsync(location, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, message);
                return Error.Failure("db.add.location.is.failed", "Ошибка сохранения локации");
            }

            return location.Id.Value;
        }
    }
}
