using Core.Abstractions;
using Core.Caching;
using Core.Validation;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace DirectoryService.Application.Features.Positions.Commands.UpdatePosition
{
    public sealed class UpdatePositionHandler : ICommandHandler<Guid, UpdatePositionCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<UpdatePositionCommand> _validator;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdatePositionHandler> _logger;

        public UpdatePositionHandler(
            ITransactionManager transactionManager,
            IPositionsRepository positionsRepository,
            IValidator<UpdatePositionCommand> validator,
            ICacheService cache,
            ILogger<UpdatePositionHandler> logger)
        {
            _transactionManager = transactionManager;
            _positionsRepository = positionsRepository;
            _validator = validator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
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

            var request = command.Request;

            var positionName = PositionName.Create(request.Name).Value;

            var activeWithTheSameNamePosition = await _positionsRepository
                .GetActivePositionByName(positionName, cancellationToken);

            if (activeWithTheSameNamePosition != null)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return PositionErrors.ActivePositionHaveSameName(positionName.Value);
            }

            var positionDescription = PositionDesription.Create(request.Description).Value;

            existingPosition.Update(positionName, positionDescription);

            var updPositionResult = await _positionsRepository.Update(existingPosition, cancellationToken);
            if (updPositionResult.IsFailure)
            {
                await _transactionManager.RollbackAsync(cancellationToken);
                return updPositionResult.Errors;
            }

            var commitResult = await _transactionManager.CommitTransactionAsync(cancellationToken);
            if (commitResult.IsFailure)
            {
                return commitResult.Errors;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_POSITION_KEY, cancellationToken);

            _logger.LogInformation("Позиция с id = {id} обновлена", positionIdValue);

            return positionIdValue;
        }
    }
}
