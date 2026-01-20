using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Positions.GetPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions.Queries.GetPositions
{
    public sealed class GetPositionsHandler : IQueryHandler<PaginationResponse<PositionDTO>, GetPositionsRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetPositionsHandler> _logger;

        public GetPositionsHandler(
            IReadDbContext readDbContext,
            ILogger<GetPositionsHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<PositionDTO>>> Handle(GetPositionsRequest request, CancellationToken cancellationToken)
        {
            int totalCount = 0;
            int totalPages = 0;
            List<PositionDTO> positions = [];
            var departmentIdValues = request.DepartmentIds;

            try
            {
                IQueryable<Position> query;
                if (departmentIdValues != null && departmentIdValues.Any())
                {
                    var departmentIds = departmentIdValues.Select(DepartmentId.Current).ToList();
                    query = _readDbContext.PositionsRead
                            .Where(p => p.DepartmentPositions.Any(dp => departmentIds.Contains(dp.DepartmentId)))
                            .OrderBy(p => p.Name.Value).ThenBy(l => l.CreatedAt);

                    totalCount = await query.CountAsync(cancellationToken);
                }
                else
                {
                    query = _readDbContext.PositionsRead;
                    if (string.IsNullOrWhiteSpace(request.Search) == false)
                    {
                        query = query.Where(l => l.Name.Value.ToLower().Contains(request.Search.ToLower()));
                    }

                    if (request.IsActive.HasValue)
                    {
                        query = query.Where(p => p.IsActive == request.IsActive);
                    }

                    totalCount = await query.CountAsync(cancellationToken);

                    query = query.Skip((request.Page - 1) * request.PageSize)
                        .Take(request.PageSize);

                    query = query.OrderBy(p => p.Name.Value).ThenBy(p => p.CreatedAt);
                }

                positions = await query.Select(p => new PositionDTO
                {
                    Id = p.Id.Value,
                    Name = p.Name.Value,
                    Description = p.Description.Value,
                    DepartmentsCount = p.DepartmentPositions.Count(),
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).ToListAsync(cancellationToken);

                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
                _logger.LogInformation("Получение списка позиций");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения данных о позициях с запросом {request}");
            }
            return new PaginationResponse<PositionDTO>(positions, totalCount, request.Page, request.PageSize, totalPages);
        }
    }
}
