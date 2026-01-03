using DirectoryService.Application.Features.Locations.Queries.GetLocations;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations
{
    public class GetLocationsTests: DirectoryBaseTests
    {
        public GetLocationsTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetLocations_with_valid_department_list_should_suceed()
        {
            // arrange
            var deptAndLocations = new[] { 2, 3, 1, 1 };
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var expectedDepts = departments.Take(2);
            var departmentIds = TestData.GetDepartmentIds(expectedDepts);
            var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);
            var locationIds = expectedDepts
                .SelectMany(dl => dl.DepartmentLocations.Select(dl => dl.Location))
                .OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt)
                .Select(l => l.Id.Value).ToList();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(departmentIdValues, cancellationToken: cancellationToken);
            var ids = result.Value.Items.Select(l => l.Id);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var getDepartments = await dbContext.DepartmentsRead
                    .Include(d => d.DepartmentLocations)
                    .Where(d => departmentIds.Contains(d.Id))
                    .ToListAsync(cancellationToken);

                var getLocationIds = getDepartments
                    .SelectMany(dl => dl.DepartmentLocations.Select(dl => dl.LocationId)).ToList();

                Assert.True(result.IsSuccess);
                Assert.NotNull(result.Value);

                Assert.NotEmpty(result.Value.Items);
                Assert.Equal(result.Value.TotalCount, result.Value.Items.Count);
                Assert.Equal(locationIds, ids);
            });
        }

        [Fact]
        public async Task GetLocations_with_not_exist_department_list_should_suceed()
        {
            // arrange
            await TestData.CreateLocations(2);

            var departmentIds = new List<DepartmentId>
            {
                DepartmentId.Create(),
                DepartmentId.Create()
            };
            var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(departmentIdValues, cancellationToken: cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task GetLocations_search_by_location_name_should_suceed()
        {
            // arrange
            var locationCount = 5;
            var expectedLocations = await TestData.CreateLocations(locationCount);
            var locationIds = expectedLocations
                .OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt)
                .Select(l => l.Id.Value).ToList();
            var locationName = "Location";

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(search: locationName, cancellationToken: cancellationToken);
            var ids = result.Value.Items.Select(l => l.Id);

            // assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Items);
            Assert.Equal(locationCount, result.Value.TotalCount);
            Assert.Equal(locationCount, result.Value.Items.Count);
            Assert.True(result.Value.Items.All(l => l.Name.Contains(locationName)));
            Assert.Equal(locationIds, ids);
        }

        [Fact]
        public async Task GetLocations_search_by_location_status_is_active_should_suceed()
        {
            // arrange
            var locationCount = 5;
            var expectedLocations = await TestData.CreateLocations(locationCount);
            var locationIds = expectedLocations
                .OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt)
                .Select(l => l.Id.Value).ToList();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(isActive: true, cancellationToken: cancellationToken);
            var ids = result.Value.Items.Select(l => l.Id);

            // assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Items);
            Assert.Equal(locationCount, result.Value.TotalCount);
            Assert.True(result.Value.Items.All(l => l.IsActive));
            Assert.Equal(locationIds, ids);
        }

        [Fact]
        public async Task GetLocations_search_by_location_status_is_not_active_should_suceed()
        {
            // arrange
            var locationCount = 2;
            var locations = await TestData.CreateLocations(locationCount);

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(isActive: false, cancellationToken: cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task GetLocations_search_by_pagination_should_suceed()
        {
            // arrange
            var locationCount = 10;
            var locations = await TestData.CreateLocations(locationCount);
            locations = locations.OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt).ToList();
            var expectedLocations = locations.TakeLast(5);
            var locationIds = expectedLocations
                .OrderBy(l => l.Name.Value).ThenBy(l => l.CreatedAt)
                .Select(l => l.Id.Value).ToList();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await GetLocations(page: 2, pageSize: 5, cancellationToken: cancellationToken);
            var ids = result.Value.Items.Select(l => l.Id);

            // assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value.Items);
            Assert.Equal(locationCount, result.Value.TotalCount);
            Assert.Equal(locationIds, ids);
        }

        private async Task<Result<PaginationResponse<LocationDTO>>> GetLocations(
            List<Guid>? departmentIds = null,
            string? search = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await TestData.ExecuteHandler(async (GetLocationsHandler sut) =>
            {
                var query = new GetLocationsRequest
                {
                    DepartmentIds = departmentIds,
                    Search = search,
                    IsActive = isActive,
                    Page = page,
                    PageSize = pageSize
                };
                return await sut.Handle(query, cancellationToken);
            });

            return result;
        }
    }
}
