using System.Linq.Expressions;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel.Result;

namespace DirectoryService.Infrastructure.Postgres.Positions
{
    internal class PositionsRepository : IPositionsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PositionsRepository> _logger;

        public PositionsRepository(ApplicationDbContext context, ILogger<PositionsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> Add(Position position, CancellationToken cancellationToken)
        {
            string name = position.Name.Value;
            try
            {
                await _context.Positions.AddAsync(position, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var positionId = position.Id.Value;

                return positionId;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName is not null)
                {
                    if (pgEx.ConstraintName.Contains("name"))
                    {
                        return PositionErrors.NameConflict(name);
                    }
                }

                _logger.LogError(ex, "Ошибка добавления позиции с наименованием {name}", name);
                return PositionErrors.DatabaseError();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции добавления позиции с наименованием {name}", name);
                return PositionErrors.OperationCancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления позиции с наименованием {name}", name);
                return PositionErrors.DatabaseError();
            }
        }

        public async Task<Position?> GetBy(Expression<Func<Position, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Positions.FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<Position?> GetByWithDepartments(Expression<Func<Position, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Positions
                .Include(p => p.DepartmentPositions)
                .FirstOrDefaultAsync(predicate, cancellationToken);

        public async Task<Result> DeactivatePositionsByDepartment(
            DepartmentId departmentId, CancellationToken cancellationToken)
        {
            var deptId = departmentId.Value;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                    $"""
                     WITH lock_positions AS (SELECT dp.position_id 
                                             FROM department_positions dp
                                             JOIN positions p ON dp.position_id = p.id
                                             WHERE dp.department_id = {deptId}
                                                AND p.is_active = true 
                                             FOR UPDATE)
                     
                     UPDATE positions
                     SET is_active = false,
                         updated_at = now()
                     WHERE id in 
                           (SELECT position_id FROM lock_positions)
                     """, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции обновления позиций у подразделения с id={deptId}", deptId);
                return DepartmentErrors.DatabaseUpdatePositionsError(deptId);
            }
        }

        public async Task<Result> Update(Position position, CancellationToken cancellationToken)
        {
            var id = position.Id.Value;
            string name = position.Name.Value;
            try
            {
                _context.Update(position);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
            {
                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName is not null)
                {
                    if (pgEx.ConstraintName.Contains("name"))
                    {
                        return PositionErrors.NameConflict(name);
                    }
                }

                _logger.LogError(ex, "Ошибка обновления позиции с наименованием {name}", name);
                return PositionErrors.DatabaseError();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Ошибка параллельного обновления позиции с наименованием {name}", name);
                return GeneralErrors.ConcurrentOperation("location");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции обновления позиции с наименованием {name}", name);
                return PositionErrors.OperationCancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Отмена операции обновления позиции с id={id}", id);
                return LocationErrors.DatabaseUpdateError(id);
            }
        }

        public async Task<Result> DeactivatePosition(PositionId positionId, CancellationToken cancellationToken)
        {
            var id = positionId.Value;
            try
            {
                await _context.Database.ExecuteSqlAsync(
                $"""
                UPDATE positions
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
                _logger.LogError(ex, "Отмена операции деактивации позиции с id={id}", id);
                return LocationErrors.DatabaseUpdateError(id);
            }
        }

        public async Task<Position?> GetActivePositionById(PositionId positionId, CancellationToken cancellationToken) =>
            await GetBy(p => p.IsActive && p.Id == positionId, cancellationToken);

        public async Task<Position?> GetActivePositionByName(PositionName positionName, CancellationToken cancellationToken) =>
            await GetBy(p => p.IsActive && p.Name.Value == positionName.Value, cancellationToken);

        public async Task<Result> AddDepartmentsToPosition(
            Position position, IEnumerable<DepartmentId> newDepartmentIds, CancellationToken cancellationToken)
        {
            var name = position.Name.Value;
            try
            {
                position.DepartmentPositions.AddRange(newDepartmentIds.Select(d => new DepartmentPosition(d, position.Id)));
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Ошибка параллельного обновления позиции с наименованием {name}", name);
                return GeneralErrors.ConcurrentOperation("location");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции добавления позиции с наименованием {name}", name);
                return PositionErrors.OperationCancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления позиции с наименованием {name}", name);
                return PositionErrors.DatabaseError();
            }
        }
    }
}
