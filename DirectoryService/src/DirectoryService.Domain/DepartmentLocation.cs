using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain
{
    public sealed class DepartmentLocation
    {
        public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
        {
            DepartmentId = departmentId;
            LocationId = locationId;
        }

        public DepartmentId DepartmentId { get; private set; }

        public LocationId LocationId { get; private set; }
    }
}
