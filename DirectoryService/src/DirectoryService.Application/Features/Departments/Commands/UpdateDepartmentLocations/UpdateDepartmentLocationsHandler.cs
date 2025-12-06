using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations
{
    public sealed class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<UpdateLocationsCommand> _validator;
        private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

        public UpdateDepartmentLocationsHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IValidator<UpdateLocationsCommand> validator,
            ILogger<UpdateDepartmentLocationsHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

            var deptId = command.DepartmentId;
            var departmentId = DepartmentId.Current(deptId);

            var department = await _departmentsRepository.GetByWithLocations(d => d.Id == departmentId && d.IsActive, cancellationToken);
            if (department == null)
            {
                transactionScope.RollBack();
                return DepartmentErrors.NotFound(deptId);
            }

            var request = command.Request;

            var locationIds = request.LocationIds.Select(LocationId.Current).ToList();
            var getLocationssResult = await _locationsRepository.GetActiveLocationsByIds(locationIds, cancellationToken);
            if (getLocationssResult.IsFailure)
            {
                transactionScope.RollBack();
                return getLocationssResult.Errors;
            }

            var locations = getLocationssResult.Value;
            var locationDepartments = locations.Select(l => new DepartmentLocation(departmentId, l.Id)).ToList();

            var updLocationsResult = department.UpdateLocations(locationDepartments);
            if (updLocationsResult.IsFailure)
            {
                transactionScope.RollBack();
                return updLocationsResult.Errors!;
            }

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Ошибка обновления локаций у подразделения с {id}", deptId);
                return DepartmentErrors.DatabaseUpdateLocationsError(deptId);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors!;
            }

            _logger.LogInformation("Локации обновлены для подразделения с {id}", deptId);

            return deptId;
        }
    }
}
