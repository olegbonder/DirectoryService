using DirectoryService.Application.Features.Departments.Commands.UpdateDepartmentLocations;
using DirectoryService.Application.Features.Departments.UpdateDepartmentLocations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments
{
    public class UpdateDepartmentLocationsTests : DirectoryBaseTests
    {
        public UpdateDepartmentLocationsTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task UpdateDepartmentLocations_with_valid_data_should_suceed()
        {
            // arrange
            var locationCount = 4;
            var oldLocationCount = 1;
            var newLocationCount = locationCount - oldLocationCount;
            var locations = await TestData.CreateLocations(locationCount);
            var oldLocations = locations.Take(oldLocationCount);
            var oldLocationIds = TestData.GetLocationIds(oldLocations);

            var newLocations = locations.TakeLast(newLocationCount);
            var newLocationIds = TestData.GetLocationIds(newLocations);
            var newLocationIdValues = TestData.GetLocationIdValues(newLocationIds);

            var department = await TestData.CreateParentDepartment("Головное подразделение", "main", oldLocations);
            var departmentId = department.Id;

            var cancellationToken = CancellationToken.None;

            // act
            var result = await UpdateDepartmentLocations(departmentId.Value, newLocationIdValues, cancellationToken);

            // assert
            await ExecuteInDb(async dbContext =>
            {
                var updateDepartment = await dbContext.Departments
                    .Include(d => d.DepartmentLocations)
                    .FirstAsync(d => d.Id == departmentId, cancellationToken);

                var departmentLocations = updateDepartment.DepartmentLocations;

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(updateDepartment);
                Assert.NotEmpty(updateDepartment.DepartmentLocations);
                Assert.Equal(newLocationCount, departmentLocations.Count);
                Assert.DoesNotContain(departmentLocations, dl => oldLocationIds.Contains(dl.LocationId));
                Assert.Contains(departmentLocations, dl => newLocationIds.Contains(dl.LocationId));
            });
        }

        [Fact]
        public async Task UpdateDepartmentLocations_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await UpdateDepartmentLocations(Guid.Empty, [Guid.Empty], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("departmentid.is.empty", error.Code);
            Assert.Equal(ErrorType.Validation, error.Type);
        }

        [Fact]
        public async Task CreateDepartment_with_not_exist_department_should_failed()
        {
            // arrange
            var location = await TestData.CreateLocation("Локация");
            var locationId = location.Id;

            var departmentId = DepartmentId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await UpdateDepartmentLocations(departmentId.Value, [locationId.Value], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        [Fact]
        public async Task UpdateDepartmentLocations_with_not_exist_newlocation_should_failed()
        {
            // arrange
            var locations = await TestData.CreateLocations(2);
            var department = await TestData.CreateParentDepartment("Головное подразделение", "main", locations);
            var departmentId = department.Id;

            var newLocationId = LocationId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await UpdateDepartmentLocations(departmentId.Value, [newLocationId.Value], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("location.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        private async Task<Result<Guid>> UpdateDepartmentLocations(
            Guid departmentId,
            List<Guid> locationsIds,
            CancellationToken cancellationToken)
        {
            var result = await ExecuteHandler(async (UpdateDepartmentLocationsHandler sut) =>
            {
                var command = new UpdateLocationsCommand(departmentId, new UpdateDepartmentLocationsRequest(locationsIds));
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }
    }
}