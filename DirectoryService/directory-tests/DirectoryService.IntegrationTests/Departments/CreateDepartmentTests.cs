using DirectoryService.Application.Features.Departments.CreateDepartment;
using DirectoryService.Application.Features.Locations.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            var locationId = await CreateLocation("Локация");

            var cancellationToken = CancellationToken.None;

            // act
            var result = await ExecuteHandler((sut) =>
            {

                var command = new CreateDepartmentCommand(new CreateDepartmentRequest("Подразделение", "main", null, new List<Guid> { locationId.Value }));

                return sut.Handle(command, cancellationToken);
            });

            // assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id.Value, result.Value);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);
            });
        }

        [Fact]
        public async Task CreateDepartment_with_invalid_data_should_failed()
        {
            // arrange
            var locationId = await CreateLocation("Локация");

            var cancellationToken = CancellationToken.None;

            // act
            var result = await ExecuteHandler((sut) =>
            {

                var command = new CreateDepartmentCommand(new CreateDepartmentRequest("", "main", null, new List<Guid> { locationId.Value }));

                return sut.Handle(command, cancellationToken);
            });

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
        }

        private async Task<LocationId> CreateLocation(string name)
        {
            return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create(
                    LocationName.Create(name),
                    LocationAddress.Create("Нижний Новгород", "улица", "10", "10", null),
                    LocationTimezone.Create("Europe/Moscow")
                );

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Value.Id;
            });
        }

        private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
        {
            await using var scope = Services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

            return await action(sut);
        }
    }
}
