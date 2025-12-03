using DirectoryService.Application.Features.Departments.Queries.GetTopDepartments;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetTopDepartments;
using DirectoryService.IntegrationTests.Infrasructure;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace DirectoryService.IntegrationTests.Departments;

public class GetTopDepartmentsTests : DirectoryBaseTests
{
    public GetTopDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTopDepartments_with_top_limit_should_suceed()
    {
         // arrange
         var cancellationToken = CancellationToken.None;

         var deptAndLocations = new[] { 1, 1, 1, 1, 1, 1 };
         var departments = await TestData.CreateDepartments(deptAndLocations);
         var departmentIds = TestData.GetDepartmentIds(departments);
         var departmentIdValues = TestData.GetDepartmentIdValues(departmentIds);

         var postionRes1 = await TestData.CreatePosition(
             "test1",
             string.Empty,
             departmentIdValues,
             cancellationToken);
         var position1Id = postionRes1.Value;

         var postionRes2 = await TestData.CreatePosition(
             "test2",
             string.Empty,
             departmentIdValues.Take(4).ToList(),
             cancellationToken);
         var position2Id = postionRes2.Value;

         var postionRes3 = await TestData.CreatePosition(
             "test3",
             string.Empty,
             departmentIdValues.Take(2).ToList(),
             cancellationToken);
         var position3Id = postionRes3.Value;

         // act
         var result = await GetTopDepartments(cancellationToken: cancellationToken);

         // assert
         await TestData.ExecuteInDb(async dbContext =>
         {
             var getDepartments = await dbContext.DepartmentsRead
                 .Select(d => new TopDepartmentDTO
                 {
                     Id = d.Id.Value,
                     Name = d.Name.Value,
                     CreatedAt = d.CreatedAt,
                     Depth = d.Depth,
                     Path = d.Path.Value,
                     PositionsCount = dbContext.PositionsRead
                         .Count(p => p.DepartmentPositions
                             .Any(dp => dp.DepartmentId == d.Id && departmentIds.Contains(dp.DepartmentId))),
                 })
                 .OrderByDescending(d => d.PositionsCount)
                 .ToListAsync(cancellationToken);

             var departmentsCount = await dbContext.DepartmentsRead.CountAsync(cancellationToken);

             Assert.True(result.IsSuccess);
             Assert.NotNull(result.Value);
             Assert.NotEmpty(result.Value.Departments);
             Assert.Equal(departmentsCount, result.Value.TotalCount);
             Assert.Equivalent(
                 getDepartments.Select(d => new { Id = d.Id, Count = d.PositionsCount }),
                 result.Value.Departments.Select(d => new { Id = d.Id, Count = d.PositionsCount }));
         });
    }

    [Fact]
    public async Task GetTopDepartments_with_no_data_should_suceed()
    {
         // arrange
         var cancellationToken = CancellationToken.None;

         // act
         var result = await GetTopDepartments(cancellationToken: cancellationToken);

         // assert
         await TestData.ExecuteInDb(async dbContext =>
         {
             int departmentsCount = await dbContext.DepartmentsRead.CountAsync(cancellationToken);

             Assert.True(result.IsSuccess);
             Assert.NotNull(result.Value);
             Assert.Empty(result.Value.Departments);
             Assert.Equal(departmentsCount, result.Value.TotalCount);
         });
    }

    private async Task<Result<GetTopDepartmentsResponse>> GetTopDepartments(
         int? limitTop = null,
         CancellationToken cancellationToken = default)
     {
         var result = await TestData.ExecuteHandler(async (GetTopDepartmentsHandler sut) =>
         {
             var query = new GetTopDepartmentsRequest { LimitTop = limitTop };
             return await sut.Handle(query, cancellationToken);
         });

         return result;
     }
}