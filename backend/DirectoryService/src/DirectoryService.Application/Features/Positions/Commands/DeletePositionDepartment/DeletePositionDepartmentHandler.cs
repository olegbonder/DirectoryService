using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Features.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

namespace DirectoryService.Application.Features.Positions.Commands.DeletePositionDepartment
{
    public sealed class DeletePositionDepartmentHandler : ICommandHandler<Guid, DeletePositionDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IValidator<DeletePositionDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<DeletePositionDepartmentHandler> _logger;

        public DeletePositionDepartmentHandler(
            ITransactionManager transactionManager,
            IPositionsRepository positionsRepository,
            IDepartmentsRepository departmentsRepository,
            IValidator<DeletePositionDepartmentCommand> validator,
            ICacheService cache,
            ILogger<DeletePositionDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(DeletePositionDepartmentCommand command, CancellationToken cancellationToken)
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

            var posId = command.PositionId;
            var positionId = PositionId.Current(posId);

            var position = await _positionsRepository.GetActivePositionById(positionId, cancellationToken);
            if (position == null)
            {
                transactionScope.RollBack();
                return PositionErrors.NotFound(posId);
            }

            var deptId = command.DepartmentId;
            var departmentId = DepartmentId.Current(deptId);

            var department = await _departmentsRepository.GetActiveDepartmentById(departmentId, cancellationToken);
            if (department == null)
            {
                transactionScope.RollBack();
                return DepartmentErrors.NotFound(deptId);
            }

            position.DepartmentPositions.Remove(new DepartmentPosition(departmentId, positionId));

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Ошибка удаления подразделений у позиции с {id}", posId);
                return DepartmentErrors.DatabaseUpdateLocationsError(posId);
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_POSITION_KEY, cancellationToken);

            _logger.LogInformation("Подразделения обновлены для позиции с {id}", posId);

            return posId;
        }
    }
}
