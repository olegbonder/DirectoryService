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
using Shared.Result;

namespace DirectoryService.Application.Features.Positions.CreatePosition
{
    public sealed class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
    {
        private readonly ITransactionManager _transactionManager;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<CreatePositionCommand> _validator;
        private readonly ILogger<CreatePositionHandler> _logger;

        public CreatePositionHandler(
            ITransactionManager transactionManager,
            IDepartmentsRepository departmentsRepository,
            IPositionsRepository positionsRepository,
            IValidator<CreatePositionCommand> validator,
            ILogger<CreatePositionHandler> logger)
        {
            _transactionManager = transactionManager;
            _departmentsRepository = departmentsRepository;
            _positionsRepository = positionsRepository;
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

            var transactionScopeResult = await _transactionManager.BeginTransaction(cancellationToken: cancellationToken);
            if (transactionScopeResult.IsFailure)
            {
                return transactionScopeResult.Errors;
            }

            using var transactionScope = transactionScopeResult.Value;

            var request = command.Request;

            var newPositionId = PositionId.Create();

            var positionName = PositionName.Create(request.Name).Value;

            var activeWithTheSameNamePosition = await _positionsRepository
                .GetBy(p => p.IsActive && p.Name == positionName, cancellationToken);

            if (activeWithTheSameNamePosition != null)
            {
                transactionScope.RollBack();
                return PositionErrors.ActivePositionHaveSameName(positionName.Value);
            }

            var positionDesription = PositionDesription.Create(request.Description).Value;

            var departmentIds = request.DepartmentIds.Select(DepartmentId.Current).ToList();
            var getDepartmentsRes = await _departmentsRepository.GetDepartmentByIds(departmentIds, cancellationToken);
            if (getDepartmentsRes.IsFailure)
            {
                transactionScope.RollBack();
                return getDepartmentsRes.Errors;
            }

            var departments = getDepartmentsRes.Value;
            var departmentPositions = departments.Select(d => new DepartmentPosition(d.Id, newPositionId)).ToList();

            var positionRes = Position.Create(newPositionId, positionName, positionDesription, departmentPositions);
            if (positionRes.IsFailure)
            {
                transactionScope.RollBack();
                return positionRes.Errors!;
            }

            var position = positionRes.Value;
            var addPositionRes = await _positionsRepository.AddAsync(position, cancellationToken);
            if (addPositionRes.IsFailure)
            {
                transactionScope.RollBack();
                return addPositionRes.Errors!;
            }

            var commitResult = transactionScope.Commit();
            if (commitResult.IsFailure)
            {
                return commitResult.Errors!;
            }

            _logger.LogInformation("Позиция с id = {id} сохранена в БД", addPositionRes.Value);

            return addPositionRes.Value;
        }
    }
}
