using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.SoftDeletePosition
{
    public sealed class SoftDeletePositionHandler : ICommandHandler<Guid, SoftDeletePositionCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<SoftDeletePositionCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<SoftDeletePositionHandler> _logger;

        public SoftDeletePositionHandler(
            ITransactionManager transactionManager,
            IPositionsRepository positionsRepository,
            IValidator<SoftDeletePositionCommand> validator,
            ICacheService cache,
            ILogger<SoftDeletePositionHandler> logger)
        {
            _transactionManager = transactionManager;
            _positionsRepository = positionsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(SoftDeletePositionCommand command, CancellationToken cancellationToken)
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

            var positionIdValue = command.PositionId;
            var positionId = PositionId.Current(positionIdValue);

            var existingPosition = await _positionsRepository.GetActivePositionById(positionId, cancellationToken);
            if (existingPosition == null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return PositionErrors.NotFound(positionIdValue);
            }

            var positionResult = await _positionsRepository.DeactivatePosition(positionId, cancellationToken);
            if (positionResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return positionResult.Errors;
            }

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                _logger.LogError("Ошибка обновления позиции с {id}", positionIdValue);
                return LocationErrors.DatabaseUpdateError(positionIdValue);
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_POSITION_KEY, cancellationToken);

            _logger.LogInformation("Позиция с id = {id} не активна", positionIdValue);

            return positionIdValue;
        }
    }
}
