using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartmentDictionary
{
    public sealed class GetDepartmentDictionaryHandler : IQueryHandler<DictionaryResponse>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetDepartmentDictionaryHandler> _logger;

        public GetDepartmentDictionaryHandler(
            IReadDbContext readDbContext,
            ILogger<GetDepartmentDictionaryHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<Result<DictionaryResponse>> Handle(CancellationToken cancellationToken)
        {
            List<DictionaryItemResponse> items = [];
            try
            {
                IQueryable<Department> query = _readDbContext.DepartmentsRead;

                items = await query
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name.Value)
                    .Select(d => new DictionaryItemResponse(d.Id.Value, d.Name.Value))
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Получение справочника департаментов");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения справочника департаментов");
            }

            return new DictionaryResponse(items);
        }
    }
}
