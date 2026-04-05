using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations
{
    public sealed class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<UpdateLocationsCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

        public UpdateDepartmentLocationsHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IValidator<UpdateLocationsCommand> validator,
            ICacheService cache,
            ILogger<UpdateDepartmentLocationsHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
            {
                return transactionResult.Errors;
            }

            var deptId = command.DepartmentId;
            var departmentId = DepartmentId.Current(deptId);

            var department = await _departmentsRepository.GetByWithLocations(d => d.Id == departmentId && d.IsActive, cancellationToken);
            if (department == null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return DepartmentErrors.NotFound(deptId);
            }

            var request = command.Request;

            var locationIds = request.LocationIds.Select(LocationId.Current).ToList();
            var getLocationsResult = await _locationsRepository.GetActiveLocationsByIds(locationIds, cancellationToken);
            if (getLocationsResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return getLocationsResult.Errors;
            }

            var locations = getLocationsResult.Value;
            var locationDepartments = locations
                .Select(l => new DepartmentLocation(departmentId, l.Id)).ToList();

            var updLocationsResult = department.UpdateLocations(locationDepartments);
            if (updLocationsResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updLocationsResult.Errors;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogError("Ошибка обновления локаций у подразделения с {id}", deptId);
                return DepartmentErrors.DatabaseUpdateLocationsError(deptId);
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_KEY, cancellationToken);

            _logger.LogInformation("Локации обновлены для подразделения с {id}", deptId);

            return deptId;
        }
    }
}
