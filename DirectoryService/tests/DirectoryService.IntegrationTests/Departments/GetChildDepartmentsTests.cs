using DirectoryService.Application.Features.Departments.Queries.GetDepartments;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.GetChildDepartments;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrasructure;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments;

public class GetChildDepartmentsTests(DirectoryTestWebFactory factory)
    : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task GetChildDepartments_with_valid_request_should_suceed()
    {
         // arrange
         var cancellationToken = CancellationToken.None;

         int[] deptAndLocations = [1, 1, 1, 1, 1, 1];
         var departments = await TestData.CreateDepartments(deptAndLocations);
         var parentId = departments[0].Id.Value;
         var location = await TestData.CreateLocation("Test", "12");
         var secondChildDeparment = await TestData.CreateDepartment("Test", "tst",
             parentId, [location.Id.Value], cancellationToken);

         // act
         var result = await GetChildDepartments(parentId, cancellationToken: cancellationToken);

         // assert
         Assert.True(result.IsSuccess);
         Assert.NotNull(result.Value);
         var getDepartments = result.Value.Departments;
         Assert.NotEmpty(getDepartments);
         Assert.Equal(2, result.Value.TotalCount);
         Assert.Equal(2, getDepartments.Count);
         var firstDepartment = getDepartments.First();
         var lastDepartment = getDepartments.Last();
         Assert.True(firstDepartment.HasMoreChildren);
         Assert.False(lastDepartment.HasMoreChildren);
         Assert.Equal(departments[1].Id.Value, firstDepartment.Id);
         Assert.Equal(secondChildDeparment.Value, lastDepartment.Id);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_with_second_page_should_suceed()
    {
         // arrange
         int[] deptAndLocations = [1, 1, 1, 1, 1, 1];
         var departments = await TestData.CreateDepartments(deptAndLocations);
         var parentId = departments[0].Id.Value;
         var cancellationToken = CancellationToken.None;

         // act
         var result = await GetChildDepartments(parentId, page: 2, cancellationToken: cancellationToken);

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

        int[] deptAndLocations = [1, 1, 1, 1, 1, 1];
        var departments = await TestData.CreateDepartments(deptAndLocations);
        var parentId = departments[0].Id.Value;
        var location = await TestData.CreateLocation("Test", "12");
        var secondParentDeparmentResult = await TestData.CreateDepartment("Test", "tst",
            parentId, [location.Id.Value], cancellationToken);
        var secondParentDeparmentId = secondParentDeparmentResult.Value;

        // act
        var result = await GetChildDepartments(parentId, page: 2, size: 1, cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        var getDepartments = result.Value.Departments;
        Assert.NotEmpty(getDepartments);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Single(getDepartments);
        Assert.Equal(secondParentDeparmentId, getDepartments.First().Id);
    }

    [Fact]
    public async Task GetRootDepartmentsTests_not_exist_parent_id_should_suceed()
    {
        // arrange
        var parentId = DepartmentId.Create();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await GetChildDepartments(parentId.Value, cancellationToken: cancellationToken);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Departments);
        Assert.Equal(0, result.Value.TotalCount);
    }

    private async Task<Result<GetChildDepartmentsResponse>> GetChildDepartments(
         Guid parentId,
         int? page = null,
         int? size = null,
         CancellationToken cancellationToken = default)
     {
         var result = await TestData.ExecuteHandler(async (GetChildDepartmentsHandler sut) =>
         {
             var request = new GetChildDepartmentsRequest()
             {
                 Pagination = new PaginationRequest { Page = page ?? 1, PageSize = size ?? 20 }
             };
             var query = new GetChildDepartmentsQuery(parentId, request);
             return await sut.Handle(query, cancellationToken);
         });

         return result;
     }
}