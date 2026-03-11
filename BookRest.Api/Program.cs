using BookRest.Api.Data;
using BookRest.Api.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencies();
builder.AddDatabase();

var app = builder.Build();

app.MigrateDb();

await app.InitializeAsync();

app.UseDevelopmentServices();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "hello bookrest");

app.Run();