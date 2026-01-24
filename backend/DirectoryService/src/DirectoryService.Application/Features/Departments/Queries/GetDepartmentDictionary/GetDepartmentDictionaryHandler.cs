using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartmentDictionary
{
    public sealed class GetDepartmentDictionaryHandler : IQueryHandler<DictionaryResponse>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ICacheService _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ILogger<GetDepartmentDictionaryHandler> _logger;

        public GetDepartmentDictionaryHandler(
            IReadDbContext readDbContext,
            ICacheService cache,
            ILogger<GetDepartmentDictionaryHandler> logger)
        {
            _readDbContext = readDbContext;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
            };
            _logger = logger;
        }

        public async Task<Result<DictionaryResponse>> Handle(CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_DEPARTMENT_DICTIONARY_KEY}";

            var cachedDepartmentDictionary = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var departmentDictionary = await GetDepartmentDictionary(cancellationToken);

                    return departmentDictionary;
                },
                cancellationToken);

            if (cachedDepartmentDictionary is null)
            {
                _logger.LogInformation($"Данные по подразделениям не найдены в кэше/БД");
            }

            return cachedDepartmentDictionary!;
        }

        private async Task<DictionaryResponse> GetDepartmentDictionary(CancellationToken cancellationToken)
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

                _logger.LogInformation("Получение справочника подразделений");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения справочника подразделений");
            }

            return new DictionaryResponse(items);
        }
    }
}
