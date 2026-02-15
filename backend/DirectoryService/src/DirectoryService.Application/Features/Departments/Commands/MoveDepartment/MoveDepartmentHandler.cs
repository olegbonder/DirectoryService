using Core.Abstractions;
using Core.Caching;
using Core.Database;
using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.MoveDepartment
{
    public sealed class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IValidator<MoveDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<MoveDepartmentHandler> _logger;

        public MoveDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            IValidator<MoveDepartmentCommand> validator,
            ICacheService cache,
            ILogger<MoveDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var deptId = command.DepartmentId;
            var departmentId = DepartmentId.Current(deptId);

            var newParentId = command.Request.ParentId;
            Department? newParentDepartment = null;

            // Проверяем, что идентификаторы с родителем - разные
            if (newParentId.HasValue && newParentId.Value == deptId)
            {
                return DepartmentErrors.ParentIdConflict();
            }

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

            if (newParentId.HasValue)
            {
                var newParentDeptId = DepartmentId.Current(newParentId.Value);

                // поиск среди дочерних подразделений
                bool hasChildDepartments = await _departmentsRepository.IsExistsChildForParent(departmentId, newParentDeptId, cancellationToken);
                if (hasChildDepartments)
                {
                    transactionScope.RollBack();
                    return DepartmentErrors.ParentIdAsChildConflict(newParentId.Value);
                }

                // поиск активного родителя
                newParentDepartment = await _departmentsRepository.GetActiveDepartmentById(newParentDeptId, cancellationToken);
                if (newParentDepartment == null)
                {
                    transactionScope.RollBack();
                    return DepartmentErrors.NotFound(newParentId.Value);
                }
            }

            // Выбираем подразделение для пессимистичной блокировки
            var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (departmentResult.IsFailure)
            {
                transactionScope.RollBack();
                return departmentResult.Errors;
            }

            var department = departmentResult.Value!;
            var oldDepartmentPath = department.Path;

            // Выбираем дочерние подразделения для пессимистичной блокировки
            await _departmentsRepository.GetChildrensWithLock(department.Path, cancellationToken);

            // Перемещаем только подразделение
            department.MoveDepartment(newParentDepartment);

            // Обновляем данные дочерних сущностей
            var updateChildrenResult = await _departmentsRepository.UpdateChildrensForMove(oldDepartmentPath, department, cancellationToken);
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
                _logger.LogError(ex, "Ошибка перемещения подразделения с {id}", deptId);
                return DepartmentErrors.DatabaseUpdateLocationsError(deptId);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_KEY, cancellationToken);

            _logger.LogInformation("Подразделениe с {id} успешно перемещено", deptId);

            return deptId;
        }
    }
}
