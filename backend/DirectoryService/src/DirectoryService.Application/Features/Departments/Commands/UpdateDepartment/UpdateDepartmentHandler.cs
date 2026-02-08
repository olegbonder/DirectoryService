using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.UpdateDepartment
{
    public sealed class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<UpdateDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateDepartmentHandler> _logger;

        public UpdateDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IValidator<UpdateDepartmentCommand> validator,
            ICacheService cache,
            ILogger<UpdateDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _locationsRepository = locationsRepository;
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

            var existingDepartment = await _departmentsRepository.GetActiveDepartmentById(departmentId, cancellationToken);
            if (existingDepartment == null)
            {
                transactionScope.RollBack();
                return DepartmentErrors.NotFound(departmentIdValue);
            }

            var request = command.Request;

            var deptName = DepartmentName.Create(request.Name).Value;

            var deptIdentifier = DepartmentIdentifier.Create(request.Identifier).Value;

            var parentId = request.ParentId;
            Department? parentDepartment = null;
            DepartmentId? parentDepartmentId = null;
            int depth = 0;
            if (parentId.HasValue)
            {
                parentDepartmentId = DepartmentId.Current(parentId.Value);
                parentDepartment = await _departmentsRepository.GetBy(d => d.Id == parentDepartmentId, cancellationToken);
                if (parentDepartment == null)
                {
                    transactionScope.RollBack();
                    return DepartmentErrors.NotFound(parentId.Value);
                }

                depth = parentDepartment.Depth + 1;
            }

            var deptPath = DepartmentPath.Create(deptIdentifier, parentDepartment).Value;

            existingDepartment.Update(parentDepartmentId, deptName, deptIdentifier, deptPath, depth);

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
