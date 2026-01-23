using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Positions
{
    public class CreatePositionTests: DirectoryBaseTests
    {
        public CreatePositionTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreatePosition_with_valid_data_should_suceed()
        {
            // arrange
            var deptLocations = new[] { 1, 2, 1 };
            var departments = await TestData.CreateDepartments(deptLocations);
            var departmentIds = TestData.GetDepartmentIds(departments);
            var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);
            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreatePosition("Менеджер", string.Empty, departmentIdValues, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var createdPosition = await dbContext.Positions
                    .Include(p => p.DepartmentPositions)
                    .FirstAsync(d => d.Id == PositionId.Current(result.Value), cancellationToken);

                var getDepartments = createdPosition.DepartmentPositions;
                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(createdPosition);
                Assert.Equal(createdPosition.Id.Value, result.Value);
                Assert.Equal(deptLocations.Length, getDepartments.Count);
                Assert.Equal(deptLocations.Length, getDepartments.Count(dp => departmentIds.Contains(dp.DepartmentId)));
            });
        }

        [Fact]
        public async Task CreatePosition_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreatePosition(string.Empty, string.Empty, [], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(2, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("position.name"));
            Assert.Contains(errorCodes, e => e.Contains("position.departmentIds"));
            Assert.True(result.Errors.All(e => e.Type == ErrorType.Validation));
        }

        [Fact]
        public async Task CreatePosition_with_same_position_name_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            var name = "test";
            var deptLocations = new[] { 1, 1 };
            var departments = await TestData.CreateDepartments(deptLocations);
            var departmentIds = TestData.GetDepartmentIds(departments);
            var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);
            await TestData.CreatePosition(name, null, departmentIdValues, cancellationToken);

            // act
            var result = await TestData.CreatePosition(name, null, departmentIdValues, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("position.name.is.conflict", error.Code);
            Assert.Equal(ErrorType.Conflict, error.Type);
        }

        [Fact]
        public async Task CreatePosition_with_no_department_should_failed()
        {
            // arrange
            var departmentId = DepartmentId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await TestData.CreatePosition("Менеджер", string.Empty, [departmentId.Value], cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }
    }
}
