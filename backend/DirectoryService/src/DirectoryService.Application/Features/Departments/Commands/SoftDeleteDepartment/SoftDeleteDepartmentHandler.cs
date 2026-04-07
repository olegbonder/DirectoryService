using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Features.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment
{
    public sealed class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

        public SoftDeleteDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IPositionsRepository positionsRepository,
            IValidator<SoftDeleteDepartmentCommand> validator,
            ICacheService cache,
            ILogger<SoftDeleteDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _positionsRepository = positionsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(SoftDeleteDepartmentCommand command, CancellationToken cancellationToken)
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

            // Выбираем подразделение для пессимистичной блокировки
            var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return departmentResult.Errors;
            }

            var department = departmentResult.Value;
            if (department == null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return DepartmentErrors.NotFound(deptId);
            }

            // Деактивируем локации и позиции
            var updateLocationsResult =
                await _locationsRepository.DeactivateLocationsByDepartment(departmentId, cancellationToken);
            if (updateLocationsResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updateLocationsResult.Errors;
            }

            var updatePositionsResult =
                await _positionsRepository.DeactivatePositionsByDepartment(departmentId, cancellationToken);
            if (updatePositionsResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updateLocationsResult.Errors;
            }

            string oldDepartmentPath = department.Path.Value;

            department.SoftDelete();

            string newDepartmentPath = department.Path.Value;

            // Выбираем дочерние подразделения для пессимистичной блокировки
            await _departmentsRepository.GetChildrensWithLock(department.Path, cancellationToken);

            // Обновляем данные дочерних сущностей
            var updateChildrenResult = await _departmentsRepository
                .UpdateChildrenPaths(oldDepartmentPath, newDepartmentPath, departmentId.Value, cancellationToken);
            if (updateChildrenResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updateChildrenResult.Errors;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogError("Ошибка обновления подразделения с {id}", deptId);
                return DepartmentErrors.DatabaseUpdateError(deptId);
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_KEY, cancellationToken);
            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_DICTIONARY_KEY, cancellationToken);

            _logger.LogInformation("Подразделение с id = {id} не активно", department.Id.Value);

            return department.Id.Value;
        }
    }
}
