using DirectoryService.Application.Features.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments
{
    public class SoftDeleteDepartmentTests(DirectoryTestWebFactory factory)
        : DirectoryBaseTests(factory)
    {
        [Fact]
        public async Task SoftDeleteDepartment_with_valid_data_should_suceed()
        {
            // arrange
            string deletedMark = "deleted_";
            var cancellationToken = CancellationToken.None;

            int[] deptAndLocations = [1, 1, 3, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var departmentIds = TestData.GetDepartmentIds(departments);
            var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);

            var positionRes1 = await TestData.CreatePosition(
                "test1",
                string.Empty,
                departmentIdValues,
                cancellationToken);
            var position1Id = positionRes1.Value;

            var positionRes2 = await TestData.CreatePosition(
                "test2",
                string.Empty,
                departmentIdValues.Take(4).ToList(),
                cancellationToken);
            var position2Id = positionRes2.Value;

            var existPositions = new List<Guid> { position1Id, position2Id };

            var positionRes3 = await TestData.CreatePosition(
                "test3",
                string.Empty,
                departmentIdValues.Take(2).ToList(),
                cancellationToken);
            var position3Id = positionRes3.Value;

            var softDeleteDepartmentIdValue = departmentIdValues[2];
            var softDeleteDepartmentId = DepartmentId.Current(softDeleteDepartmentIdValue);

            // act
            var result = await SoftDeleteDepartment(softDeleteDepartmentIdValue, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var softDeleteDepartment = await dbContext.Departments
                    .Include(d => d.DepartmentLocations)
                    .ThenInclude(dl => dl.Location)
                    .FirstAsync(d => d.Id == softDeleteDepartmentId, cancellationToken);
                string softDeleteDepartmentPath = softDeleteDepartment.Path.Value;
                var deptLocations = softDeleteDepartment.DepartmentLocations.Select(dl => dl.Location).ToList();

                var deptPositions = await dbContext.Positions
                    .Where(
                        d => d.DepartmentPositions
                        .Any(dp => dp.DepartmentId == softDeleteDepartmentId))
                    .ToListAsync(cancellationToken);

                var childrenDepartments = await dbContext.Departments.FromSql(
                    $"""
                     SELECT * FROM departments
                     WHERE path <@ {softDeleteDepartmentPath}::ltree
                     and path != {softDeleteDepartmentPath}::ltree
                     """).ToListAsync(cancellationToken);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.Equal(softDeleteDepartmentIdValue, result.Value);
                Assert.False(softDeleteDepartment.IsActive);
                Assert.NotNull(softDeleteDepartment.DeletedAt);
                Assert.Equal(3, deptLocations.Count);
                Assert.True(deptLocations.All(l => l.IsActive == false));
                Assert.Equal(2, deptPositions.Count);
                Assert.Equal(2, deptPositions.Count(p => existPositions.Contains(p.Id.Value)));
                Assert.DoesNotContain(deptPositions, p => p.Id.Value == position3Id);
                Assert.True(deptPositions.All(l => l.IsActive == false));
                Assert.True(childrenDepartments.All(d => d.Path.Value.StartsWith(deletedMark)));
            });
        }

        [Fact]
        public async Task CreateDepartment_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await SoftDeleteDepartment(Guid.Empty, cancellationToken);

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
            var departmentId = DepartmentId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await SoftDeleteDepartment(departmentId.Value, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        private async Task<Result<Guid>> SoftDeleteDepartment(
            Guid departmentId,
            CancellationToken cancellationToken)
        {
            var result = await TestData.ExecuteHandler(async (SoftDeleteDepartmentHandler sut) =>
            {
                var command = new SoftDeleteDepartmentCommand(departmentId);
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }
    }
}
