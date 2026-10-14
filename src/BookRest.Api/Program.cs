using BookRest.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddApiServices();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // Learn more at https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(static builder =>
{
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin();
});

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/scalar"));

app.UseAuthorization();

app.MapControllers();

app.Run();
