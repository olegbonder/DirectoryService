using System.Linq.Expressions;
using DirectoryService.Application.Features.Departments;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Departments
{
    public class DepartmentsRepository : IDepartmentsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentsRepository> _logger;

        public DepartmentsRepository(ApplicationDbContext context, ILogger<DepartmentsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddAsync(Department department, CancellationToken cancellationToken)
        {
            var name = department.Name.Value;
            try
            {
                await _context.Departments.AddAsync(department, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Подразделение с id = {id} сохранена в БД", department.Id.Value);

                return department.Id.Value;
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex, "Отмена операции добавления подразделения с наименованием {name}", name);
                return GeneralErrors.OperationCancelled("location");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления подразделения с наименованием {name}", name);
                return Error.Failure("department.database.error", "Ошибка сохранения подразделения");
            }
        }

        public async Task<Department> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken) =>
            await _context.Departments.FirstAsync(predicate, cancellationToken);
    }
}
