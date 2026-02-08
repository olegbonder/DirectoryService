using System.Text.Json;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.GetDepartmentDictionary;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartmentDictionary
{
    public sealed class GetDepartmentDictionaryHandler : IQueryHandler<PaginationResponse<DictionaryItemResponse>, GetDepartmentDictionaryRequest>
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

        public async Task<Result<PaginationResponse<DictionaryItemResponse>>> Handle(GetDepartmentDictionaryRequest query, CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_DEPARTMENT_DICTIONARY_KEY}{JsonSerializer.Serialize(query)}";

            var cachedDepartmentDictionary = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var departmentDictionary = await GetDepartmentDictionary(query, cancellationToken);

                    return departmentDictionary;
                },
                cancellationToken);

            if (cachedDepartmentDictionary is null)
            {
                _logger.LogInformation($"Данные по подразделениям не найдены в кэше/БД");
            }

            return cachedDepartmentDictionary!;
        }

        private async Task<PaginationResponse<DictionaryItemResponse>> GetDepartmentDictionary(GetDepartmentDictionaryRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<DictionaryItemResponse> items = [];
            var departmentIdValues = request.DepartmentIds;
            try
            {
                IQueryable<Department> query = _readDbContext.DepartmentsRead;
                if (departmentIdValues != null && departmentIdValues.Any())
                {
                    var departmentIds = departmentIdValues.Select(DepartmentId.Current).ToList();
                    query = query.Where(d => departmentIds.Contains(d.Id));
                }

                query = query.Where(d => d.IsActive);
                if (string.IsNullOrWhiteSpace(request.Search) == false)
                {
                    query = query.Where(l => l.Name.Value.ToLower().Contains(request.Search.ToLower()));
                }

                if (request.ShowOnlyParents)
                {
                    query = query.Where(d => d.Children.Any());
                }

                totalCount = await query.CountAsync(cancellationToken);

                query = query.Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                items = await query
                    .OrderBy(d => d.Name.Value)
                    .Select(d => new DictionaryItemResponse(d.Id.Value, d.Name.Value))
                    .ToListAsync(cancellationToken);

                totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                _logger.LogInformation("Получение справочника подразделений");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения справочника подразделений");
            }

            return new PaginationResponse<DictionaryItemResponse>(items, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
