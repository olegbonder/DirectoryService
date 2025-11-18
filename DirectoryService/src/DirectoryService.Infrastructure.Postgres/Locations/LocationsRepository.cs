using DirectoryService.Application.Features.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
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
            var name = location.Name.Value;
            try
            {
                await _context.AddAsync(location, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return location.Id.Value;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName is not null)
                {
                    if (pgEx.ConstraintName.Contains("name"))
                    {
                        return LocationErrors.NameConflict(name);
                    }

                    if (pgEx.ConstraintName.Contains("address"))
                    {
                        return LocationErrors.AddressConflict();
                    }
                }

                _logger.LogError(ex, "Ошибка добавления локации с наименованием {name}", name);
                return LocationErrors.DatabaseError();
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции добавления локации с наименованием {name}", name);
                return GeneralErrors.OperationCancelled("location");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления локации с наименованием {name}", name);
                return LocationErrors.DatabaseError();
            }
        }

        public async Task<Result<IReadOnlyCollection<Location>>> GetLocationByIds(List<LocationId> locationIds, CancellationToken cancellationToken)
        {
            var locations = await _context.Locations.Where(l => locationIds.Contains(l.Id)).ToListAsync(cancellationToken);
            var result = CheckLocationsByIds(locationIds, locations);
            return result;
        }

        public async Task<Result<IReadOnlyCollection<Location>>> GetActiveLocationByIds(List<LocationId> locationIds, CancellationToken cancellationToken)
        {
            var locations = await _context.Locations.Where(l => l.IsActive && locationIds.Contains(l.Id)).ToListAsync(cancellationToken);
            var result = CheckLocationsByIds(locationIds, locations);
            return result;
        }

        private Result<IReadOnlyCollection<Location>> CheckLocationsByIds(List<LocationId> locationIds, List<Location> locations)
        {
            var notFoundLocationIds = locationIds.Except(locations.Select(l => l.Id));
            if (notFoundLocationIds.Any())
            {
                var errors = notFoundLocationIds.Select(l => LocationErrors.NotFound(l.Value));

                return new Errors(errors);
            }

            return locations;
        }
    }
}
