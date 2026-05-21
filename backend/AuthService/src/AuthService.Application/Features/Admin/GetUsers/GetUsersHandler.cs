using System.Text.Json;
using AuthService.Application.Database;
using AuthService.Contracts.Dtos.Admin.GetUsers;
using AuthService.Domain;
using Core.Abstractions;
using Core.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SharedKernel.PaginationAndOrder;
using SharedKernel.Result;

namespace AuthService.Application.Features.Admin.GetUsers
{
    public sealed class GetUsersHandler : IQueryHandler<PaginationResponse<UserDTO>, GetUsersRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ICacheService _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly ILogger<GetUsersHandler> _logger;

        public GetUsersHandler(
            IReadDbContext readDbContext,
            ICacheService cache,
            ILogger<GetUsersHandler> logger)
        {
            _readDbContext = readDbContext;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(Constants.SLIDING_EXPIRATION_TIME_FROM_MINUTES)
            };
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<UserDTO>>> Handle(GetUsersRequest query, CancellationToken cancellationToken)
        {
            string key = $"{Constants.PREFIX_USER_KEY}{JsonSerializer.Serialize(query)}";

            var cachedUsers = await _cache.GetOrSetAsync(key, _cacheOptions,
                async () =>
                {
                    var users = await GetUsers(query, cancellationToken);

                    return users;
                },
                cancellationToken);

            if (cachedUsers is null)
            {
                _logger.LogInformation($"Данные по пользователям с запросом {query} не найдены в кэше/БД");
            }

            return cachedUsers!;
        }

        private async Task<PaginationResponse<UserDTO>> GetUsers(GetUsersRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<UserDTO> users = [];

            try
            {
                IQueryable<ApplicationUser> query = _readDbContext.UsersRead;

                if (string.IsNullOrWhiteSpace(request.Name) == false)
                {
                    query = query.Where(u => $"{u.LastName} {u.FirstName}".ToLower().Contains(request.Name.ToLower()));
                }

                if (string.IsNullOrWhiteSpace(request.Email) == false)
                {
                    query = query.Where(u => u.Email.ToLower().Contains(request.Email.ToLower()));
                }

                totalCount = await query.CountAsync(cancellationToken);

                query = query.Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);

                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                users = await query.Select(u => new UserDTO(
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email ?? string.Empty,
                    u.IsActive)).ToListAsync(cancellationToken);

                _logger.LogInformation("Получение списка пользователей с параметрами {Request}", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения данных о пользователях с запросом {request}");
            }

            return new PaginationResponse<UserDTO>(users, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
