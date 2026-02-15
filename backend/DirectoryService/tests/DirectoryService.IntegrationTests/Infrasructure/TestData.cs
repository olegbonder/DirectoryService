using DirectoryService.Application.Features.Departments.Commands.CreateDepartment;
using DirectoryService.Application.Features.Locations.Commands.CreateLocation;
using DirectoryService.Application.Features.Positions.Commands.CreatePosition;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Positions.CreatePosition;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Result;

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

        public async Task<Result<Guid>> CreateLocation(
            string name,
            AddressDTO address,
            string timezone,
            CancellationToken cancellationToken)
        {
            var result = await ExecuteHandler(async (CreateLocationHandler sut) =>
            {
                var command = new CreateLocationCommand(new CreateLocationRequest(name, address, timezone));
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }

        public async Task<Location> CreateLocation(string name, string houseNumber = "10")
        {
            var addressDto = new AddressDTO("Россия", "Нижний Новгород", "улица", "10", houseNumber);
            string timeZone = "Europe/Moscow";

            var locationIdResult = await CreateLocation(name, addressDto, timeZone, default);
            var location = await GetLocation(locationIdResult.Value);

            return location!;
        }

        public async Task<List<Location>> CreateLocations(int newLocationCount = 2)
        {
            var list = new List<Location>();
            for (int i = 0; i < newLocationCount; i++)
            {
                var addressDto = new AddressDTO("Россиия", "Нижний Новгород", "улица", "10", i.ToString());
                string timeZone = "Europe/Moscow";
                var locationIdRes = await CreateLocation($"Location {i}", addressDto, timeZone, default);
                var location = await GetLocation(locationIdRes.Value);
                list.Add(location!);
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

        public async Task<Result<Guid>> CreatePosition(
            string name,
            string? description,
            List<Guid> departmentIds,
            CancellationToken cancellationToken)
        {
            var result = await ExecuteHandler(async (CreatePositionHandler sut) =>
            {
                var command = new CreatePositionCommand(new CreatePositionRequest(name, description, departmentIds));
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }

        private async Task<Department> GetDepartment(Guid departmentId)
        {
            return await ExecuteInDb(async dbcontext =>
            {
                var department = await dbcontext.Departments
                    .Include(d => d.DepartmentLocations)
                    .ThenInclude(dl => dl.Location)
                    .FirstAsync(d => d.Id == DepartmentId.Current(departmentId));

                return department;
            });
        }

        private async Task<Location> GetLocation(Guid locationId)
        {
            return await ExecuteInDb(async dbcontext =>
            {
                var location = await dbcontext.Locations
                    .FirstAsync(d => d.Id == LocationId.Current(locationId));

                return location;
            });
        }
    }
}
