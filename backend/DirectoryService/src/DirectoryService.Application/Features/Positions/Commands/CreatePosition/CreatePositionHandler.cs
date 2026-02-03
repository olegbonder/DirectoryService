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

namespace DirectoryService.Application.Features.Positions.Commands.CreatePosition
{
    public sealed class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly ICacheService _cache;
        private readonly IValidator<CreatePositionCommand> _validator;
        private readonly ILogger<CreatePositionHandler> _logger;

        public CreatePositionHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            IPositionsRepository positionsRepository,
            ICacheService cache,
            IValidator<CreatePositionCommand> validator,
            ILogger<CreatePositionHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _positionsRepository = positionsRepository;
            _cache = cache;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
        {
            var validResult = await _validator.ValidateAsync(command, cancellationToken);
            if (validResult.IsValid == false)
            {
                return validResult.ToList();
            }

            var request = command.Request;

            var newPositionId = PositionId.Create();

            var positionName = PositionName.Create(request.Name).Value;

            var activeWithTheSameNamePosition = await _positionsRepository
                .GetActivePositionByName(positionName, cancellationToken);

            if (activeWithTheSameNamePosition != null)
            {
                return PositionErrors.ActivePositionHaveSameName(positionName.Value);
            }

            var positionDesription = PositionDesription.Create(request.Description).Value;

            var departmentIds = request.DepartmentIds.Select(DepartmentId.Current).ToList();
            var getDepartmentsRes = await _departmentsRepository.GetDepartmentByIds(departmentIds, cancellationToken);
            if (getDepartmentsRes.IsFailure)
            {
                return getDepartmentsRes.Errors;
            }

            var departments = getDepartmentsRes.Value;
            var departmentPositions = departments.Select(d => new DepartmentPosition(d.Id, newPositionId)).ToList();

            var positionRes = Position.Create(newPositionId, positionName, positionDesription, departmentPositions);
            if (positionRes.IsFailure)
            {
                return positionRes.Errors!;
            }

            var position = positionRes.Value;
            var addPositionRes = await _positionsRepository.Add(position, cancellationToken);
            if (addPositionRes.IsFailure)
            {
                return addPositionRes.Errors!;
            }

            await _cache.RemoveByPrefixAsync(Constants.PREFIX_POSITION_KEY, cancellationToken);

            _logger.LogInformation("Позиция с id = {id} сохранена в БД", addPositionRes.Value);

            return addPositionRes.Value;
        }
    }
}
