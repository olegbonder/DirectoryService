using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface IReadDbContext
    {
        IQueryable<Location> LocationsRead { get; }

        IQueryable<Position> PositionsRead { get; }

        IQueryable<Department> DepartmentsRead { get; }
    }
}
