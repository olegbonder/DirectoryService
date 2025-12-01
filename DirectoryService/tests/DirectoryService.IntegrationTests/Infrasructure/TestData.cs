using DirectoryService.Application.Features.Departments.Commands.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Infrasructure
{
    public class TestData
    {
        private readonly IServiceProvider _services;

        public TestData(IServiceProvider Services)
        {
            _services = Services;
        }

        public async Task<T> ExecuteInDb<T>(Func<ApplicationDbContext, Task<T>> action)
        {
            await using var scope = _services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await action(dbContext);
        }

        public async Task ExecuteInDb(Func<ApplicationDbContext, Task> action)
        {
            await using var scope = _services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await action(dbContext);
        }

        public async Task<T> ExecuteHandler<T, TCommandHandler>(Func<TCommandHandler, Task<T>> action)
            where TCommandHandler : class
        {
            await using var scope = _services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<TCommandHandler>();

            return await action(sut);
        }

        public async Task<Location> CreateLocation(string name, string houseNumber = "10")
        {
            return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create(
                    LocationName.Create(name),
                    LocationAddress.Create("Нижний Новгород", "улица", "10", houseNumber, null),
                    LocationTimezone.Create("Europe/Moscow"));

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Value;
            });
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

        public List<DepartmentId> GetDepartmentIds(IEnumerable<Department> departments) =>
            departments.Select(l => l.Id).ToList();

        public List<Guid> GetDepartmentIdValues(IEnumerable<DepartmentId> departmentIds) =>
            departmentIds.Select(l => l.Value).ToList();

        public async Task<Result<Guid>> CreateDepartment(
            string name,
            string identifier,
            Guid? parentDepartmentId,
            List<Guid> locationIds,
            CancellationToken cancellationToken)
        {
            var result = await ExecuteHandler(async (CreateDepartmentHandler sut) =>
            {
                var command = new CreateDepartmentCommand(new CreateDepartmentRequest(name, identifier, parentDepartmentId, locationIds));
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }

        public async Task<Department> CreateParentDepartment(string name, string identifier, IEnumerable<Location> locations)
        {
            return await ExecuteInDb(async dbcontext =>
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

                dbcontext.Departments.Add(department);
                await dbcontext.SaveChangesAsync();

                return department.Value;
            });
        }

        public async Task<List<Department>> CreateDepartments(int[] deptAndLocations)
        {
            var departments = new List<Department>();
            Department? parentDept = null;
            int locationCount = 0;
            for (int i = 0; i <= deptAndLocations.Length; i++)
            {
                locationCount = +i;
            }

            var locations = await CreateLocations(locationCount);
            var allLocationsCount = 0;
            for (int i = 0; i < deptAndLocations.Length; i++)
            {
                var deptLocationCount = deptAndLocations[i];
                var deptLocations = locations.Skip(allLocationsCount).Take(deptLocationCount).ToList();

                var locationIdValues = GetLocationIdValues(GetLocationIds(deptLocations));
                if (i == 0)
                {
                    var parentResult = await CreateDepartment("Test root department", "root", null, locationIdValues, default);
                    parentDept = await GetDepartment(parentResult.Value);
                }
                else
                {
                    // чтобы прошла валидация регулярного выражения @"^[a-z]+$" при создании VO "DepartmentIdentifier"
                    char letter = 'a';
                    string identifier = $"identifier{new string(letter, i)}";

                    var parentDeptId = parentDept!.Id.Value;
                    var deptResult = await CreateDepartment($"Test child_{i} department", identifier, parentDeptId, locationIdValues, default);
                    parentDept = await GetDepartment(deptResult.Value);
                }

                departments.Add(parentDept!);
                allLocationsCount = +deptLocationCount;
            }

            return departments;
        }

        private async Task<Department?> GetDepartment(Guid departmentId)
        {
            return await ExecuteInDb(async dbcontext =>
            {
                var department = await dbcontext.Departments
                    .Include(d => d.DepartmentLocations)
                    .ThenInclude(dl => dl.Location)
                    .FirstOrDefaultAsync(d => d.Id == DepartmentId.Current(departmentId));

                return department;
            });
        }
    }
}
