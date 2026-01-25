using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Result;

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

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

            var positionIdValue = command.PositionId;
            var positionId = PositionId.Current(positionIdValue);

            var existingPosition = await _positionsRepository.GetActivePositionById(positionId, cancellationToken);
            if (existingPosition == null)
            {
                transactionScope.RollBack();
                return PositionErrors.NotFound(positionIdValue);
            }

            var positionResult = await _positionsRepository.DeactivatePosition(positionId, cancellationToken);
            if (positionResult.IsFailure)
            {
                transactionScope.RollBack();
                return positionResult.Errors;
            }

            try
            {
                await _transactionManager.SaveChanges(cancellationToken);
            }
            catch (Exception ex)
            {
                transactionScope.RollBack();
                _logger.LogError(ex, "Ошибка обновления позиции с {id}", positionIdValue);
                return LocationErrors.DatabaseUpdateError(positionIdValue);
            }

            var commitResult = transactionScope.Commit();
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
