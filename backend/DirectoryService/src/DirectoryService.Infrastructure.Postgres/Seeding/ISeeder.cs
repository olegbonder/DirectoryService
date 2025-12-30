namespace DirectoryService.Infrastructure.Postgres.Seeding
{
    public interface ISeeder
    {
        Task SeedAsync();
    }
}
