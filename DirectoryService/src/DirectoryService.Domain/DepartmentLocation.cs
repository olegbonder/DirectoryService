namespace DirectoryService.Domain
{
    public class DepartmentLocation
    {
        public DepartmentLocation(Guid departmentId, Guid locationId)
        {
            DepartmentId = departmentId;
            LocationId = locationId;
        }

        public Guid DepartmentId { get; private set; }

        public Guid LocationId { get; private set; }
    }
}
