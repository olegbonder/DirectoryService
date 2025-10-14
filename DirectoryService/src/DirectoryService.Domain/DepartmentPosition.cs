namespace DirectoryService.Domain
{
    public class DepartmentPosition
    {
        public DepartmentPosition(Guid departmentId, Guid positionId)
        {
            DepartmentId = departmentId;
            PositionId = positionId;
        }

        public Guid DepartmentId { get; set; }

        public Guid PositionId { get; set; }
    }
}
