using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.DeletePositionDepartment
{
    public sealed class DeletePositionDepartmentHandler : ICommandHandler<Guid, DeletePositionDepartmentCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<DeletePositionDepartmentCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<DeletePositionDepartmentHandler> _logger;

        public DeletePositionDepartmentHandler(
            ITransactionManager transactionManager,
            IPositionsRepository positionsRepository,
            IValidator<DeletePositionDepartmentCommand> validator,
            ICacheService cache,
            ILogger<DeletePositionDepartmentHandler> logger)
        {
            _transactionManager = transactionManager;
            _positionsRepository = positionsRepository;
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

            var posId = command.PositionId;
            var positionId = PositionId.Current(posId);

            var position = await _positionsRepository.GetByWithDepartments(p => p.IsActive && p.Id == positionId, cancellationToken);
            if (position == null)
            {
                return PositionErrors.NotFound(posId);
            }

            var deptId = command.DepartmentId;
            var departmentId = DepartmentId.Current(deptId);
            var departmentPosition = position.DepartmentPositions.FirstOrDefault(dp => dp.DepartmentId == departmentId && dp.PositionId == positionId);
            if (departmentPosition == null)
            {
                return DepartmentErrors.NotFound(deptId);
            }

            position.DepartmentPositions.Remove(departmentPosition);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError("Ошибка удаления подразделений у позиции с {id}", posId);
                return DepartmentErrors.DatabaseUpdateLocationsError(posId);
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_POSITION_KEY, cancellationToken);

            _logger.LogInformation("Подразделения обновлены для позиции с {id}", posId);

            return posId;
        }
    }
}
