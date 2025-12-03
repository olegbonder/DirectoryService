using DirectoryService.Application.Features.Departments.Queries.GetDepartments;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.GetRootDepartments;
using DirectoryService.IntegrationTests.Infrasructure;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments;

public class GetRootDepartmentsTests : DirectoryBaseTests
{
    public GetRootDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRootDepartmentsTests_with_valid_request_should_suceed()
    {
         // arrange
         var cancellationToken = CancellationToken.None;

         var deptAndLocations = new[] { 1, 1, 1, 1, 1, 1 };
         var departments = await TestData.CreateDepartments(deptAndLocations);
         var location = await TestData.CreateLocation("Test", "12");
         var secondChildDeparment = await TestData.CreateDepartment("Test", "tst",
             departments[0].Id.Value, [location.Id.Value], cancellationToken);

         // act
         var result = await GetRootDepartments(cancellationToken: cancellationToken);

         // assert
         Assert.True(result.IsSuccess);
         Assert.NotNull(result.Value);
         var getRootDepartments = result.Value.Departments;
         Assert.NotEmpty(getRootDepartments);
         Assert.Equal(1, result.Value.TotalCount);
         Assert.Single(getRootDepartments);
         var getRootDepartment = getRootDepartments.First();
         Assert.Equal(departments.First().Id.Value, getRootDepartment.Id);
         Assert.True(getRootDepartment.HasMoreChildren);
         var getChildren = getRootDepartment.Children;
         Assert.Equal(2, getChildren.Count);
         Assert.True(getChildren.First().HasMoreChildren);
         Assert.False(getChildren.Last().HasMoreChildren);
         Assert.Equal(secondChildDeparment.Value, getChildren.Last().Id);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_with_prefetch_should_suceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var deptAndLocations = new[] { 1, 1, 1, 1, 1, 1 };
        var departments = await TestData.CreateDepartments(deptAndLocations);
        var location = await TestData.CreateLocation("Test", "12");
        var secondChildDeparment = await TestData.CreateDepartment("Test", "tst",
            departments[0].Id.Value, [location.Id.Value], cancellationToken);

        // act
        var result = await GetRootDepartments(prefetch: 1, cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var getRootDepartments = result.Value.Departments;
        Assert.NotEmpty(getRootDepartments);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(getRootDepartments);
        var getRootDepartment = getRootDepartments.First();
        Assert.Equal(departments.First().Id.Value, getRootDepartment.Id);
        Assert.True(getRootDepartment.HasMoreChildren);
        var getChildren = getRootDepartment.Children;
        Assert.Single(getChildren);
        Assert.True(getChildren.First().HasMoreChildren);
        Assert.Equal(departments[1].Id.Value, getChildren.Last().Id);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_with_second_page_should_suceed()
    {
         // arrange
         var deptAndLocations = new[] { 1, 1, 1, 1, 1, 1 };
         var departments = await TestData.CreateDepartments(deptAndLocations);

         var cancellationToken = CancellationToken.None;

         // act
         var result = await GetRootDepartments(page: 2, cancellationToken: cancellationToken);

         // assert
         Assert.True(result.IsSuccess);
         Assert.Empty(result.Value.Departments);
         Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_with_first_page_size_equal_one_should_suceed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var deptAndLocations = new[] { 1, 1, 1, 1, 1, 1 };
        await TestData.CreateDepartments(deptAndLocations);
        var location = await TestData.CreateLocation("Test", "12");
        var secondParentDeparmentResult = await TestData.CreateDepartment("Test", "tst",
            null, [location.Id.Value], cancellationToken);
        var secondParentDeparmentId = secondParentDeparmentResult.Value;

        // act
        var result = await GetRootDepartments(page: 2, size: 1, cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        var rootDepartments = result.Value.Departments;
        Assert.NotEmpty(rootDepartments);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Single(rootDepartments);
        Assert.Equal(secondParentDeparmentId, rootDepartments.First().Id);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_no_data_should_suceed()
    {
        // arrange

        var cancellationToken = CancellationToken.None;

        // act
        var result = await GetRootDepartments(cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0, result.Value.TotalCount);
    }

    private async Task<Result<GetRootDepartmentsResponse>> GetRootDepartments(
         int? page = null,
         int? size = null,
         int? prefetch = null,
         CancellationToken cancellationToken = default)
     {
         var result = await TestData.ExecuteHandler(async (GetRootDepartmentsHandler sut) =>
         {
             var query = new GetRootDepartmentsRequest()
             {
                 Prefetch = prefetch,
                 Pagination = new PaginationRequest
                 {
                     Page = page ?? 1,
                     PageSize = size ?? 20
                 }
             };
             return await sut.Handle(query, cancellationToken);
         });

         return result;
     }
}