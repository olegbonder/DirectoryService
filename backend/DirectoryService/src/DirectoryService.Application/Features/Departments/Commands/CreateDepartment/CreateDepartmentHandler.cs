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
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Departments.Commands.CreateDepartment
{
    public sealed class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IValidator<CreateDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateDepartmentHandler> _logger;

        public CreateDepartmentHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IValidator<CreateDepartmentCommand> validator,
            ICacheService cache,
            ILogger<CreateDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var request = command.Request;

            var newDeptId = DepartmentId.Create();

            var deptName = DepartmentName.Create(request.Name).Value;

            var deptIdentifier = DepartmentIdentifier.Create(request.Identifier).Value;

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

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

            var locationIds = request.LocationIds.Select(LocationId.Current).ToList();
            var getLocationsRes = await _locationsRepository.GetLocationsByIds(locationIds, cancellationToken);
            if (getLocationsRes.IsFailure)
            {
                transactionScope.RollBack();
                return getLocationsRes.Errors;
            }

            var locations = getLocationsRes.Value;
            var locationDepartments = locations.Select(l => new DepartmentLocation(newDeptId, l.Id)).ToList();

            var departmentRes = Department.Create(newDeptId, parentDepartmentId, deptName, deptIdentifier, deptPath, depth, locationDepartments);
            if (departmentRes.IsFailure)
            {
                transactionScope.RollBack();
                return departmentRes.Errors;
            }

            var addDepartmentRes = await _departmentsRepository.AddAsync(departmentRes.Value, cancellationToken);
            if (addDepartmentRes.IsFailure)
            {
                return addDepartmentRes.Errors;
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_KEY, cancellationToken);
            await _cache.RemoveByPrefixAsync(Constants.PREFIX_DEPARTMENT_DICTIONARY_KEY, cancellationToken);

            _logger.LogInformation("Подразделение с id = {id} сохранена в БД", addDepartmentRes.Value);

            return addDepartmentRes.Value;
        }
    }
}
