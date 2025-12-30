using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace DirectoryService.Infrastructure.Postgres.Seeding
{
    internal class Seeder : ISeeder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<Seeder> _logger;
        private readonly Random _rnd = new();

        private static T UnwrapResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return result.Value;

            // If creation failed — бросаем, т.к. сгенерированные данные должны быть валидными.
            var errors = result.Errors;
            throw new ApplicationException($"Domain factory returned failure during seeding: {errors}");
        }

        private static string RandomAlphaNumeric(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }

        private static string RandomLatin(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            var rnd = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }

        private static class SeedConstants
        {
            public const int LOCATIONS_COUNT = 10;
            public const int DEPARTMENTS_COUNT = 15;
            public const int POSITIONS_COUNT = 20;

            public const int MAX_LOCATIONS_PER_DEPARTMENT = 3;
            public const int MAX_CHILDREN_PER_DEPARTMENT = 3;
            public const int MAX_DEPT_DEPTH = 3;
        }

        public Seeder(ApplicationDbContext dbContext, ILogger<Seeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding...");

            try
            {
                await SeedData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while seeding data");
                throw;
            }

            _logger.LogInformation("Database seeding completed.");
        }

        private async Task SeedData()
        {
            // Выполняем всё в одной транзакции
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Clearing database tables...");

                // Очистка всех таблиц с учётом FK (Postgres)
                await _dbContext.Database.ExecuteSqlAsync(
                    $"""
                    TRUNCATE TABLE department_positions, department_locations, departments, positions, locations RESTART IDENTITY CASCADE;
                    """);

                _logger.LogInformation("Database cleared. Generating seed data...");

                // 1. Locations
                var locations = new List<Location>();
                var timezones = TimeZoneInfo.GetSystemTimeZones().Select(t => t.Id).ToList();
                for (var i = 0; i < SeedConstants.LOCATIONS_COUNT; i++)
                {
                    var name = $"loc-{i}-{RandomAlphaNumeric(6)}";
                    var lnRes = LocationName.Create(name);
                    var addrRes = LocationAddress.Create($"Country{i}", $"City{i}", $"Street{i}", $"{i + 1}", null);
                    var tz = timezones[_rnd.Next(timezones.Count)];
                    var tzRes = LocationTimezone.Create(tz);

                    var location = UnwrapResult(Location.Create(UnwrapResult(lnRes), UnwrapResult(addrRes), UnwrapResult(tzRes)));
                    locations.Add(location);
                }

                await _dbContext.Locations.AddRangeAsync(locations);
                await _dbContext.SaveChangesAsync();

                // 2. Departments (hierarchy + department locations)
                var departments = new List<Department>();

                for (var i = 0; i < SeedConstants.DEPARTMENTS_COUNT; i++)
                {
                    // Создаём id заранее (нужно для DepartmentLocation)
                    var deptId = DepartmentId.Create();

                    // Выбираем родителя случайно (или null)
                    Department? parent = null;
                    if (departments.Any() && _rnd.NextDouble() < 0.6) // 60% вероятность назначить родителя
                    {
                        // не создаём слишком глубокую иерархию
                        var possibleParents = departments.Where(d => d.Depth < SeedConstants.MAX_DEPT_DEPTH).ToList();
                        if (possibleParents.Any())
                        {
                            parent = possibleParents[_rnd.Next(possibleParents.Count)];
                        }
                    }

                    var identifierString = RandomLatin(8).ToLower();
                    var nameString = $"department-{i}-{RandomAlphaNumeric(4)}";

                    var identifierRes = DepartmentIdentifier.Create(identifierString);
                    var nameRes = DepartmentName.Create(nameString);

                    var depth = parent == null ? 0 : parent.Depth + 1;
                    var pathRes = DepartmentPath.Create(UnwrapResult(identifierRes), parent);

                    // locations for dept (at least 1)
                    var locCount = _rnd.Next(1, Math.Min(SeedConstants.MAX_LOCATIONS_PER_DEPARTMENT, locations.Count) + 1);
                    var chosenLocations = locations.OrderBy(_ => _rnd.Next()).Take(locCount).ToList();
                    var deptLocations = chosenLocations.Select(l => new DepartmentLocation(deptId, l.Id)).ToList();

                    var deptRes = Department.Create(
                        deptId,
                        parent?.Id,
                        UnwrapResult(nameRes),
                        UnwrapResult(identifierRes),
                        UnwrapResult(pathRes),
                        depth,
                        deptLocations);

                    var department = UnwrapResult(deptRes);

                    // EF navigation: department.DepartmentLocations should be set by constructor; DepartmentLocation objects already reference ids
                    departments.Add(department);
                }

                await _dbContext.Departments.AddRangeAsync(departments);
                await _dbContext.SaveChangesAsync();

                // 3. Positions (each position must have at least one department)
                var positions = new List<Position>();
                for (var i = 0; i < SeedConstants.POSITIONS_COUNT; i++)
                {
                    var posId = PositionId.Create();
                    var nameString = $"position-{i}-{RandomAlphaNumeric(4)}";
                    var nameRes = PositionName.Create(nameString);

                    // optional description
                    var desc = _rnd.NextDouble() < 0.5 ? $"desc {RandomAlphaNumeric(20)}" : null;
                    var descRes = PositionDesription.Create(desc);

                    // choose 1..3 departments
                    var deptCount = _rnd.Next(1, Math.Min(3, departments.Count) + 1);
                    var chosen = departments.OrderBy(_ => _rnd.Next()).Take(deptCount).ToList();
                    var deptPositions = chosen.Select(d => new DepartmentPosition(d.Id, posId)).ToList();

                    var posRes = Position.Create(posId, UnwrapResult(nameRes), UnwrapResult(descRes), deptPositions);
                    var position = UnwrapResult(posRes);

                    // Заполняем навигацию (чтобы EF добавил записи в department_positions)
                    foreach (var dp in deptPositions)
                    {
                        position.DepartmentPositions.Add(dp);
                    }

                    positions.Add(position);
                }

                await _dbContext.Positions.AddRangeAsync(positions);
                await _dbContext.SaveChangesAsync();

                // Коммит транзакции
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Seeding finished. Inserted: {locations} locations, {departments} departments, {positions} positions",
                    locations.Count, departments.Count, positions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seeding failed, rolling back transaction");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
