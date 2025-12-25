using DirectoryService.Application.Features.Locations;
using DirectoryService.Domain.Departments;
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
            string name = location.Name.Value;
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

        public async Task<Result<IReadOnlyCollection<Location>>> GetLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken)
        {
            var locations = await _context.Locations
                .Where(l => locationIds.Contains(l.Id)).ToListAsync(cancellationToken);
            var result = CheckLocationsByIds(locationIds, locations);
            return result;
        }

        public async Task<Result<IReadOnlyCollection<Location>>> GetActiveLocationsByIds(
            List<LocationId> locationIds, CancellationToken cancellationToken)
        {
            var locations = await _context.Locations
                .Where(l => l.IsActive && locationIds.Contains(l.Id)).ToListAsync(cancellationToken);
            var result = CheckLocationsByIds(locationIds, locations);
            return result;
        }

        public async Task<Result> DeactivateLocationsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken)
        {
            var deptId = departmentId.Value;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                    $"""
                     WITH lock_locations AS (SELECT dl.location_id 
                                             FROM department_locations dl
                                             JOIN locations l ON dl.location_id = l.id
                                             WHERE dl.department_id = {deptId}
                                                AND l.is_active = true 
                                             FOR UPDATE)
                     
                     UPDATE locations
                     SET is_active = false,
                         updated_at = now()
                     WHERE id in 
                           (SELECT location_id FROM lock_locations)
                     """, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции обновления локаций у подразделения с id={deptId}", deptId);
                return DepartmentErrors.DatabaseUpdateLocationsError(deptId);
            }
        }

        private Result<IReadOnlyCollection<Location>> CheckLocationsByIds(
            List<LocationId> locationIds, List<Location> locations)
        {
            var notFoundLocationIds = locationIds.Except(locations.Select(l => l.Id)).ToList();
            if (notFoundLocationIds.Any())
            {
                var errors = notFoundLocationIds.Select(l => LocationErrors.NotFound(l.Value));

                return new Errors(errors);
            }

            return locations;
        }
    }
}
