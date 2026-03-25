using Core.Abstractions;
using DirectoryService.Contracts.Departments.AttachDepartmentVideo;

namespace DirectoryService.Application.Features.Departments.Commands.AttachDepartmentVideo
{
    public record AttachDepartmentVideoCommand(Guid DepartmentId, AttachDepartmentVideoRequest Request) : ICommand;
}
