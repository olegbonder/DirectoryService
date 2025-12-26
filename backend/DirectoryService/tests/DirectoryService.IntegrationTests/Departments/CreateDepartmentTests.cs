using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments
{
    public class CreateDepartmentTests: DirectoryBaseTests
    {
        public CreateDepartmentTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreateDepartment_with_valid_data_should_suceed()
        {
            // arrange
            var newLocationCount = 2;
            var locations = await TestData.CreateLocations(newLocationCount);
            var locationIds = TestData.GetLocationIds(locations);
            var locationIdValues = TestData.GetLocationIdValues(locationIds);

            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreateDepartment("Подразделение", "main", null, locationIdValues, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var createdDepartment = await dbContext.Departments
                    .Include(d => d.DepartmentLocations)
                    .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(createdDepartment);
                Assert.Equal(createdDepartment.Id.Value, result.Value);
                Assert.Equal(0, createdDepartment.Depth);
                Assert.Equal(createdDepartment.Identifier.Value, createdDepartment.Path.Value);
                Assert.Equal(newLocationCount, createdDepartment.DepartmentLocations.Count);
                Assert.Equal(newLocationCount, createdDepartment.DepartmentLocations.Count(dl => locationIds.Contains(dl.LocationId)));
            });
        }

        [Fact]
        public async Task CreateDepartment_with_parent_should_suceed()
        {
            // arrange
            var newLocationCount = 2;
            var locations = await TestData.CreateLocations(newLocationCount);
            var locationIds = TestData.GetLocationIds(locations);
            var locationIdValues = TestData.GetLocationIdValues(locationIds);

            var parentDepartment = await CreateParentDepartment("Головное подразделение", "main", locations);
            var parentDepartmentId = parentDepartment.Id;
            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreateDepartment("Подразделение", "dev", parentDepartmentId.Value, locationIdValues, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

                var parentDepartment = await dbContext.Departments
                    .FirstAsync(d => d.Id == parentDepartmentId, cancellationToken);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(department);
                Assert.Equal(department.Id.Value, result.Value);
                Assert.NotNull(parentDepartment);
                Assert.NotNull(department.ParentId);
                Assert.Equal(parentDepartment.Id, department.ParentId);
                Assert.Equal(parentDepartment.Depth + 1, department.Depth);
                Assert.Equal($"{parentDepartment.Path.Value}.{department.Identifier.Value}", department.Path.Value);
            });
        }

        [Fact]
        public async Task CreateDepartment_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreateDepartment(string.Empty, string.Empty, null, [], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(3, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("department.name"));
            Assert.Contains(errorCodes, e => e.Contains("department.identifier"));
            Assert.Contains(errorCodes, e => e.Contains("department.locationIds"));
            Assert.True(result.Errors.All(e => e.Type == ErrorType.Validation));
        }

        [Fact]
        public async Task CreateDepartment_with_not_exist_location_should_failed()
        {
            // arrange
            var locationId = LocationId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreateDepartment("Подразделение", "main", null, [locationId.Value], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("location.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        [Fact]
        public async Task CreateDepartment_with_not_exist_parent_department_should_failed()
        {
            // arrange
            var location = await TestData.CreateLocation("Локация");
            var locationId = location.Id;

            var parentDepartmentId = DepartmentId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreateDepartment("Подразделение", "main", parentDepartmentId.Value, [locationId.Value], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        private async Task<Department> CreateParentDepartment(string name, string identifier, IEnumerable<Location> locations)
        {
            return await TestData.ExecuteInDb(async dbcontext =>
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
    }
}
