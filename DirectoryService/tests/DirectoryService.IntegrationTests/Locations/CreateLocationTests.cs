using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Locations;

public class CreateLocationTests: DirectoryBaseTests
{
    public CreateLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLocation_with_valid_data_should_suceed()
    {
        // arrange
        var name = "Локация";
        var addressDto = new AddressDTO("Россия", "Нижний Новгород", "улица", "10","10");
        string timeZone = "Europe/Moscow";

        var cancellationToken = CancellationToken.None;

        // act
        var result = await TestData.CreateLocation(name, addressDto, timeZone, cancellationToken);

        // assert
        await TestData.ExecuteInDb(async dbContext =>
        {
            var createdLocation= await dbContext.Locations
                .FirstAsync(d => d.Id == LocationId.Current(result.Value), cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(createdLocation);
            Assert.Equal(createdLocation.Id.Value, result.Value);
            Assert.Equal(name, createdLocation.Name.Value);
        });
    }

    [Fact]
    public async Task CreateLocation_with_invalid_request_should_failed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var addressDto = new AddressDTO(
            "Россия", "Нижний Новгород", String.Empty, "10", null);

        // act
        var result = await TestData.CreateLocation(string.Empty, addressDto, "test", cancellationToken);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(3, result.Errors.Count());

        var errorCodes = result.Errors.Select(e => e.Code).ToList();
        Assert.Contains(errorCodes, e => e.Contains("location.name"));
        Assert.Contains(errorCodes, e => e.Contains("location.address.street"));
        Assert.Contains(errorCodes, e => e.Contains("location.timezone"));
        Assert.True(result.Errors.All(e => e.Type == ErrorType.Validation));
    }
}