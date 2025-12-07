using DirectoryService.Application.Features.Departments.Commands.MoveDepartment;
using DirectoryService.Contracts.Departments.MoveDepartment;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments
{
    public class MoveDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
    {
        [Fact]
        public async Task MoveDepartment_with_valid_data_move_to_root_path_should_suceed()
        {
            // arrange
            int[] deptAndLocations = [1, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var moveDepartment = departments[2];
            var moveDepartmentId = moveDepartment.Id;
            var moveDepartmentIdentifier = moveDepartment.Identifier;
            var moveDepartmentPath = moveDepartment.Path;
            int moveDepartmentDepth = moveDepartment.Depth;

            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(moveDepartmentId.Value, null, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var movedDepartment = await dbContext.Departments
                    .FirstAsync(d => d.Id == moveDepartmentId, cancellationToken);

                var movedChidren = await dbContext.Departments
                    .Where(d => d.ParentId == moveDepartmentId).ToListAsync(cancellationToken);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(movedDepartment);
                Assert.Equal(movedDepartment.Id.Value, result.Value);
                Assert.Equal(0, movedDepartment.Depth);
                Assert.NotEqual(moveDepartmentDepth, movedDepartment.Depth);
                Assert.Equal(moveDepartmentIdentifier.Value, movedDepartment.Path.Value);
                Assert.NotEqual(moveDepartmentPath, movedDepartment.Path);
                Assert.True(movedChidren.All(c => c.Depth == 1));
                Assert.True(movedChidren.All(c => c.Path.Value.StartsWith(moveDepartmentIdentifier.Value)));
            });
        }

        [Fact]
        public async Task MoveDepartment_with_valid_data_move_to_child_path_should_suceed()
        {
            // arrange
            int[] deptAndLocations = [1, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var moveDepartment = departments[3];
            var moveDepartmentId = moveDepartment.Id;
            var moveDepartmentIdentifier = moveDepartment.Identifier;
            var moveDepartmentPath = moveDepartment.Path;
            int moveDepartmentDepth = moveDepartment.Depth;

            var parentDeptId = departments[1].Id;

            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(moveDepartmentId.Value, parentDeptId.Value, cancellationToken);

            // assert
            await TestData.ExecuteInDb(async dbContext =>
            {
                var parentDepartment = await dbContext.Departments
                    .FirstAsync(d => d.Id == parentDeptId, cancellationToken);

                var movedDepartment = await dbContext.Departments
                    .FirstAsync(d => d.Id == moveDepartmentId, cancellationToken);

                var movedChidren = await dbContext.Departments
                    .Where(d => d.ParentId == moveDepartmentId).ToListAsync(cancellationToken);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);

                Assert.NotNull(movedDepartment);
                Assert.Equal(movedDepartment.Id.Value, result.Value);
                Assert.Equal(parentDepartment.Depth + 1, movedDepartment.Depth);
                Assert.NotEqual(moveDepartmentDepth, movedDepartment.Depth);
                Assert.StartsWith(parentDepartment.Path.Value, movedDepartment.Path.Value);
                Assert.NotEqual(moveDepartmentPath, movedDepartment.Path);
                Assert.True(movedChidren.All(c => c.Depth == parentDepartment.Depth + 2));
                string parentPathForChildren = $"{parentDepartment.Path.Value}.{moveDepartmentIdentifier.Value}";
                Assert.True(movedChidren.All(c => c.Path.Value.StartsWith(parentPathForChildren)));
            });
        }

        [Fact]
        public async Task MoveDepartment_with_invalid_request_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(Guid.Empty, null, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("departmentid.is.empty", error.Code);
            Assert.Equal(ErrorType.Validation, error.Type);
        }

        [Fact]
        public async Task MoveDepartment_conflict_parent_id_should_failed()
        {
            // arrange
            var departmentId = DepartmentId.Create();
            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(departmentId.Value, departmentId.Value, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.parent_id.conflict", error.Code);
            Assert.Equal(ErrorType.Failure, error.Type);
        }

        [Fact]
        public async Task MoveDepartment_move_to_parent_id_as_child_should_failed()
        {
            // arrange
            int[] deptAndLocations = [1, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var moveDepartmentId = departments[2].Id;

            var parentAsChildDeptId = departments[3].Id;
            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(moveDepartmentId.Value, parentAsChildDeptId.Value, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.parent_id.as.child.conflict", error.Code);
            Assert.Equal(ErrorType.Failure, error.Type);
        }

        [Fact]
        public async Task MoveDepartment_with_not_exist_parent_department_should_failed()
        {
            // arrange
            int[] deptAndLocations = [1, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var moveDepartmentId = departments[1].Id;

            var parentDepartmentId = DepartmentId.Create();

            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(moveDepartmentId.Value, parentDepartmentId.Value, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        [Fact]
        public async Task MoveDepartment_with_not_exist_department_should_failed()
        {
            // arrange
            int[] deptAndLocations = [1, 1, 1, 1];
            var departments = await TestData.CreateDepartments(deptAndLocations);
            var moveDepartmentId = DepartmentId.Create();
            var parentDepartmentId = departments[1].Id;

            var cancellationToken = CancellationToken.None;

            // act
            var result = await MoveDepartment(moveDepartmentId.Value, parentDepartmentId.Value, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Single(result.Errors);

            var error = result.Errors.First();
            Assert.Equal("department.not.found", error.Code);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        private async Task<Result<Guid>> MoveDepartment(
            Guid departmentId,
            Guid? parentDepartmentId,
            CancellationToken cancellationToken)
        {
            var result = await TestData.ExecuteHandler(async (MoveDepartmentHandler sut) =>
            {
                var command = new MoveDepartmentCommand(departmentId, new MoveDepartmentRequest(parentDepartmentId));
                return await sut.Handle(command, cancellationToken);
            });

            return result;
        }
    }
}
