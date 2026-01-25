using System.Text.Json;
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

        public async Task<Result<Guid>> Add(Location location, CancellationToken cancellationToken)
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

        public async Task<Result> Update(Location location, CancellationToken cancellationToken)
        {
            string name = location.Name.Value;
            try
            {
                _context.Update(location);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
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

                _logger.LogError(ex, "Ошибка обновления локации с наименованием {name}", name);
                return LocationErrors.DatabaseError();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Ошибка параллельного обновления локации с наименованием {name}", name);
                return GeneralErrors.ConcurrentOperation("location");
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции обновления локации с наименованием {name}", name);
                return GeneralErrors.OperationCancelled("location");
            }
            catch (Exception ex)
            {
                var locId = location.Id.Value;
                _logger.LogError(ex, "Ошибка обновления локации с {id}", locId);
                return LocationErrors.DatabaseUpdateError(locId);
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

        public async Task<Result> UpdateLocation(Location location, CancellationToken cancellationToken)
        {
            var id = location.Id.Value;
            var name = location.Name.Value;
            var address = JsonSerializer.Serialize(location.Address);
            var timezone = location.Timezone.Value;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                $"""
                UPDATE locations
                     SET name = {name},
                     address = {address}::jsonb,
                     timezone = {timezone},
                     updated_at = now()
                     WHERE id = {id} 
                    AND is_active = true
                """, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции обновления локации с id={id}", id);
                return LocationErrors.DatabaseUpdateError(id);
            }
        }

        public async Task<Result> DeactivateLocation(LocationId locationId, CancellationToken cancellationToken)
        {
            var id = locationId.Value;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                $"""
                UPDATE locations
                     SET is_active = false,
                         updated_at = now(),
                         deleted_at = now()
                     WHERE id = {id} 
                    AND is_active = true
                """, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции деактивации локации с id={id}", id);
                return LocationErrors.DatabaseUpdateError(id);
            }
        }

        public async Task<Location?> GetActiveLocationById(LocationId locationId, CancellationToken cancellationToken)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.IsActive && l.Id == locationId, cancellationToken);

            return location;
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
