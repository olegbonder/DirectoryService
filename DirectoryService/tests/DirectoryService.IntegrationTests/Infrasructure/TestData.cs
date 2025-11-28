using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres;

namespace DirectoryService.IntegrationTests.Infrasructure
{
    public class TestData
    {
        private readonly ApplicationDbContext _dbContext;

        public TestData(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Location> CreateLocation(string name, string houseNumber = "10")
        {
            var location = Location.Create(
                    LocationName.Create(name),
                    LocationAddress.Create("Нижний Новгород", "улица", "10", houseNumber, null),
                    LocationTimezone.Create("Europe/Moscow"));

            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync();

            return location.Value;
        }

        public async Task<List<Location>> CreateLocations(int newLocationCount = 2)
        {
            var list = new List<Location>();
            for (int i = 0; i < newLocationCount; i++)
            {
                var location = await CreateLocation($"Location {i}", $"{i}");
                list.Add(location);
            }

            return list;
        }

        public List<LocationId> GetLocationIds(IEnumerable<Location> locations) =>
            locations.Select(l => l.Id).ToList();

        public List<Guid> GetLocationIdValues(IEnumerable<LocationId> locationIds) =>
            locationIds.Select(l => l.Value).ToList();

        public async Task<Department> CreateParentDepartment(string name, string identifier, IEnumerable<Location> locations)
        {
            var departmentId = DepartmentId.Create();
            var departmentIdentifier = DepartmentIdentifier.Create(identifier);
            var locationDepartments = locations.Select(l => new DepartmentLocation(departmentId, l.Id)).ToList();
            var department = Department.Create(
                departmentId,
                null,
                DepartmentName.Create(name),
                departmentIdentifier,
                DepartmentPath.Create(departmentIdentifier),
                0,
                locationDepartments);

            _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync();

            return department.Value;
        }

        public async Task<Department> CreateChildDepartment(string name, string identifier, Department parent, IEnumerable<Location> locations)
        {
            var departmentId = DepartmentId.Create();
            var departmentIdentifier = DepartmentIdentifier.Create(identifier);
            var locationDepartments = locations.Select(l => new DepartmentLocation(departmentId, l.Id)).ToList();
            var department = Department.Create(
                departmentId,
                parent.Id,
                DepartmentName.Create(name),
                departmentIdentifier,
                DepartmentPath.Create(departmentIdentifier, parent),
                parent.Depth + 1,
                locationDepartments);

            _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync();

            return department.Value;
        }

        public async Task<List<Department>> CreateDepartments(int level)
        {
            var departments = new List<Department>();
            var locations = await CreateLocations(level);
            Department? parentDept = null;
            for (int i = 0; i < level; i++)
            {
                var location = locations[i];
                if (i == 0)
                {
                    parentDept = await CreateParentDepartment("Test root department", "root", [location]);
                }
                else
                {
                    // чтобы прошла валидация регулярного выражения @"^[a-z]+$" при создании VO "DepartmentIdentifier"
                    char letter = 'a';
                    string identifier = $"identifier{new string(letter, i)}";

                    parentDept = await CreateChildDepartment($"Test child_{i} department", identifier, parentDept!, [location]);
                }

                departments.Add(parentDept);
            }

            return departments;
        }
    }
}
