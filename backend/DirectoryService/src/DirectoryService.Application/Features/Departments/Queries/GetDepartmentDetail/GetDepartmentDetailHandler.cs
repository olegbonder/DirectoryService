using Core.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.GetDepartment;
using DirectoryService.Domain.Departments;
using FileService.Contracts;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Queries.GetDepartmentDetail
{
    public sealed class GetDepartmentDetailHandler : IQueryHandler<DepartmentDetailDTO?, GetDepartmentRequest>
    {
        private readonly IReadDbContext _readDbContext;
        private readonly IFileCommunicationService _fileCommunicationService;
        private readonly ILogger<GetDepartmentDetailHandler> _logger;

        public GetDepartmentDetailHandler(
            IReadDbContext readDbContext,
            IFileCommunicationService fileCommunicationService,
            ILogger<GetDepartmentDetailHandler> logger)
        {
            _readDbContext = readDbContext;
            _fileCommunicationService = fileCommunicationService;
            _logger = logger;
        }

        public async Task<Result<DepartmentDetailDTO?>> Handle(GetDepartmentRequest request, CancellationToken cancellationToken)
        {
            DepartmentDetailDTO? department = null;
            try
            {
                var departmentId = DepartmentId.Current(request.DepartmentId);
                department = await _readDbContext.DepartmentsRead
                .Where(d => d.Id == departmentId)
                .Select(d => new DepartmentDetailDTO
                {
                    Id = d.Id.Value,
                    ParentId = d.ParentId == null ? null : d.ParentId.Value,
                    Name = d.Name.Value,
                    Identifier = d.Identifier.Value,
                    Path = d.Path.Value,
                    Depth = d.Depth,
                    Locations = d.DepartmentLocations
                        .Select(dl => new DictionaryItemResponse(dl.Location.Id.Value, dl.Location.Name.Value)).ToList(),
                    Positions = _readDbContext.PositionsRead
                        .Where(p => p.DepartmentPositions.Any(dp => dp.DepartmentId == d.Id))
                        .Select(p => p.Name.Value).ToList(),
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt,
                    Video = d.VideoId.HasValue ? new MediaDto
                    {
                        Id = d.VideoId.Value
                    } : null
                }).FirstOrDefaultAsync(cancellationToken);

                if (department != null && department.Video != null)
                {
                    var mediaAssetResult = await _fileCommunicationService
                        .GetMediaAssetInfo(department.Video.Id, cancellationToken);
                    if (mediaAssetResult.IsFailure)
                        return mediaAssetResult.Errors;
                    var mediaAsset = mediaAssetResult.Value;
                    if (mediaAsset.Id == department.Video.Id)
                    {
                        department.Video = new MediaDto
                        {
                            Id = mediaAsset.Id,
                            Status = mediaAsset.Status,
                            Url = mediaAsset.DownloadUrl,
                        };
                    }
                }

                _logger.LogInformation("Получение информации о департаменте с id={DepartmentId}", request.DepartmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения департамента с id={DepartmentId}", request.DepartmentId);
            }

            return department;
        }
    }
}
