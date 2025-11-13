using System.Linq.Expressions;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Result;

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

        public async Task<Result<Guid>> AddAsync(Position position, CancellationToken cancellationToken)
        {
            var name = position.Name.Value;
            try
            {
                await _context.Positions.AddAsync(position, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var positionId = position.Id.Value;
                _logger.LogInformation("Позиция с id = {id} сохранена в БД", positionId);

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
    }
}
