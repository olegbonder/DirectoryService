using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment
{
    public sealed class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
        private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

        public SoftDeleteDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IPositionsRepository positionsRepository,
            IValidator<SoftDeleteDepartmentCommand> validator,
            ILogger<SoftDeleteDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _positionsRepository = positionsRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
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

            // Выбираем подразделение для пессимистичной блокировки
            var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
            {
                transactionScope.RollBack();
                return departmentResult.Errors;
            }

            var department = departmentResult.Value;
            if (department == null)
            {
                transactionScope.RollBack();
                return DepartmentErrors.NotFound(deptId);
            }

            // Деактивируем локации и позиции
            var updateLocationsResult =
                await _locationsRepository.DeactivateLocationsByDepartment(departmentId, cancellationToken);
            if (updateLocationsResult.IsFailure)
            {
                transactionScope.RollBack();
                return updateLocationsResult.Errors;
            }

            var updatePositionsResult =
                await _positionsRepository.DeactivatePositionsByDepartment(departmentId, cancellationToken);
            if (updatePositionsResult.IsFailure)
            {
                transactionScope.RollBack();
                return updateLocationsResult.Errors;
            }

            string oldDepartmentPath = department.Path.Value;

            department.SoftDelete();

            string newDepartmentPath = department.Path.Value;

            // Выбираем дочерние подразделения для пессимистичной блокировки
            await _departmentsRepository.GetChildrensWithLock(department.Path, cancellationToken);

            // Обновляем данные дочерних сущностей
            var updateChildrenResult = await _departmentsRepository
                .UpdateChildrenAndParentPaths(oldDepartmentPath, newDepartmentPath, departmentId.Value, cancellationToken);
            if (updateChildrenResult.IsFailure)
            {
                transactionScope.RollBack();
                return updateChildrenResult.Errors;
            }

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Ошибка обновления подразделения с {id}", deptId);
                return DepartmentErrors.DatabaseUpdateError(deptId);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            _logger.LogInformation("Подразделение с id = {id} не активно", department.Id.Value);

            return department.Id.Value;
        }
    }
}
