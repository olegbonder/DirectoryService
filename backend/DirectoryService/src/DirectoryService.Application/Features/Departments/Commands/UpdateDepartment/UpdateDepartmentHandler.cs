using Core.Abstractions;
using Core.Caching;
using Core.Database;
using Core.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartment
{
    public sealed class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IValidator<UpdateDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateDepartmentHandler> _logger;

        public UpdateDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            IValidator<UpdateDepartmentCommand> validator,
            ICacheService cache,
            ILogger<UpdateDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
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

            var departmentIdValue = command.Id;
            var departmentId = DepartmentId.Current(departmentIdValue);

            var existingDepartmentRes = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);
            if (existingDepartmentRes.IsFailure)
            {
                transactionScope.RollBack();
                return DepartmentErrors.NotFound(departmentIdValue);
            }

            var existingDepartment = existingDepartmentRes.Value!;
            var request = command.Request;

            var deptName = DepartmentName.Create(request.Name).Value;

            var deptIdentifier = DepartmentIdentifier.Create(request.Identifier).Value;

            var oldDepartmentPath = existingDepartment.Path;

            // Выбираем дочерние подразделения для пессимистичной блокировки
            await _departmentsRepository.GetChildrensWithLock(oldDepartmentPath, cancellationToken);

            Department? parentDepartment = null;
            if (existingDepartment.ParentId != null)
            {
                parentDepartment = await _departmentsRepository.GetActiveDepartmentById(existingDepartment.ParentId, cancellationToken);
            }

            var newDepartmentPath = DepartmentPath.Create(deptIdentifier, parentDepartment).Value;

            // Обновляем данные дочерних сущностей
            var updateChildrenResult = await _departmentsRepository.UpdateChildrenPaths(
                oldDepartmentPath.Value,
                newDepartmentPath.Value,
                departmentIdValue,
                cancellationToken);
            if (updateChildrenResult.IsFailure)
            {
                transactionScope.RollBack();
                return updateChildrenResult.Errors;
            }

            existingDepartment.Update(deptName, deptIdentifier);

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления подразделения с {id}", departmentIdValue);
                return DepartmentErrors.DatabaseUpdateError(departmentIdValue);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_KEY, cancellationToken);
            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_DICTIONARY_KEY, cancellationToken);

            _logger.LogInformation("Подразделение с id = {id} обновлено", departmentIdValue);

            return departmentIdValue;
        }
    }
}
