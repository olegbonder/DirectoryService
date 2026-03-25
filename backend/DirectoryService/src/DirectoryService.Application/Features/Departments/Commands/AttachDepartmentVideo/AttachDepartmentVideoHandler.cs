using Core.Abstractions;
using Core.Database;
using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FileService.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.AttachDepartmentVideo
{
    public sealed class AttachDepartmentVideoHanlder : ICommandHandler<Guid, AttachDepartmentVideoCommand>
    {
        private readonly ILogger<AttachDepartmentVideoHanlder> _logger;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IFileCommunicationService _fileCommunicationService;
        private readonly IValidator<AttachDepartmentVideoCommand> _validator;

        public AttachDepartmentVideoHanlder(
            ILogger<AttachDepartmentVideoHanlder> logger,
            IDepartmentsRepository departmentsRepository,
            ITransactionManager transactionManager,
            IFileCommunicationService fileCommunicationService,
            IValidator<AttachDepartmentVideoCommand> validator)
        {
            _logger = logger;
            _departmentsRepository = departmentsRepository;
            _transactionManager = transactionManager;
            _fileCommunicationService = fileCommunicationService;
            _validator = validator;
        }

        public async Task<Result<Guid>> Handle(
            AttachDepartmentVideoCommand command,
            CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var departmentIdValue = command.DepartmentId;
            var departmentId = DepartmentId.Current(departmentIdValue);
            Guid? videoId = command.Request.VideoId;
            if (videoId.HasValue)
            {
                var existsResult =
                    await _fileCommunicationService.CheckMediaAssetExists(videoId.Value, cancellationToken);
                if (existsResult.IsFailure)
                    return existsResult.Errors;

                if (!existsResult.Value.Exists)
                    return DepartmentErrors.VideoAssetNotFound(videoId.Value);
            }

            var department = await _departmentsRepository
                .GetActiveDepartmentById(departmentId, cancellationToken);
            if (department == null)
                return DepartmentErrors.NotFound(departmentIdValue);

            department.UpdateVideo(videoId);

            await _transactionManager.SaveChanges(cancellationToken);

            _logger.LogInformation("Updated video for lesson {Id}", departmentIdValue);

            return departmentIdValue;
        }
    }
}