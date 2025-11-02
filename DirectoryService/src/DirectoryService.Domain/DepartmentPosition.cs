using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain
{
    public sealed class DepartmentPosition
    {
        public DepartmentPosition(DepartmentId departmentId, PositionId positionId)
        {
            DepartmentId = departmentId;
            PositionId = positionId;
        }

        public DepartmentId DepartmentId { get; set; }

        public PositionId PositionId { get; set; }
    }
}
