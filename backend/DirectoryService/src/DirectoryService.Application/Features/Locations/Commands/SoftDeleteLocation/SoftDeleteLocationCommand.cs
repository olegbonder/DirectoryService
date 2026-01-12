using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Features.Locations.Commands.SoftDeleteLocation;

public record SoftDeleteLocationCommand(Guid LocationId) : ICommand;