using DirectoryService.Infrastructure.DataBase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ApplicationDbContext>(sp =>
    new ApplicationDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb") !));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService.Web"));
}

app.MapControllers();

app.Run();
