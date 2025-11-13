using DirectoryService.Application.Abstractions;
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
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly IValidator<CreatePositionCommand> _validator;
        private readonly ILogger<CreatePositionHandler> _logger;

        public CreatePositionHandler(
            IDepartmentsRepository departmentsRepository,
            IPositionsRepository positionsRepository,
            IValidator<CreatePositionCommand> validator,
            ILogger<CreatePositionHandler> logger)
        {
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

            var request = command.Request;

            var newPositionId = PositionId.Create();

            var positionName = PositionName.Create(request.Name).Value;

            var activeWithTheSameNamePosition = await _positionsRepository
                .GetBy(p => p.Name.Value == request.Name, cancellationToken);

            if (activeWithTheSameNamePosition != null)
            {
                return PositionErrors.ActivePositionHaveSameName();
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
            var addPositionRes = await _positionsRepository.AddAsync(position, cancellationToken);
            if (addPositionRes.IsFailure)
            {
                return addPositionRes.Errors!;
            }

            _logger.LogInformation("Позиция с {id} добавлена", addPositionRes.Value);

            return addPositionRes.Value;
        }
    }
}
