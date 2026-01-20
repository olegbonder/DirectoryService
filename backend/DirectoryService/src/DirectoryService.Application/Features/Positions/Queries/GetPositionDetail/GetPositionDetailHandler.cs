using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts.Positions.GetPosition;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions.Queries.GetPositionDetail
{
    public sealed class GetPositionDetailHandler : IQueryHandler<PositionDetailDTO?, GetPositionRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetPositionDetailHandler> _logger;

        public GetPositionDetailHandler(
            IReadDbContext readDbContext,
            ILogger<GetPositionDetailHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<Result<PositionDetailDTO?>> Handle(GetPositionRequest request, CancellationToken cancellationToken)
        {
            PositionDetailDTO? position = null;
            try
            {
                var positionId = PositionId.Current(request.PositionId);
                position = await _readDbContext.PositionsRead
                .Where(p => p.Id == positionId)
                .Select(p => new PositionDetailDTO
                {
                    Id = p.Id.Value,
                    Name = p.Name.Value,
                    Description = p.Description.Value,
                    Departments = _readDbContext.DepartmentsRead
                        .Where(d => p.DepartmentPositions.Any(dp => dp.DepartmentId == d.Id))
                        .Select(d => d.Name.Value).ToList(),
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefaultAsync(cancellationToken);

                _logger.LogInformation("Получение информации о позициии с id={PositionId}", request.PositionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения позиции с id={PositionId}", request.PositionId);
            }

            return position;
        }
    }
}
