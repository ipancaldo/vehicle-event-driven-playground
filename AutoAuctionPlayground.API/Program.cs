using System.Text.Json.Serialization;
using AutoAuctionPlayground.API.Extensions;
using AutoAuctionPlayground.Infrastructure.Extensions;
using AutoAuctionPlayground.Infrastructure.Persistence;
using AutoAuctionPlayground.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Enums travel as their names ("Published", "EUR"), matching how they are stored.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCQRS();
builder.Services.AddInfrastructure(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:7222",
            builder.Configuration["Frontend:Url"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AutoAuctionDbContext>();

    // Reference data — safe in every environment, no-ops once already seeded.
    await VehicleMakeSeeder.SeedAsync(db);

    // Fixture data (demo companies/users/listings) — Development only, never touches a real DB.
    if (app.Environment.IsDevelopment())
    {
        var users = await CompanyUserSeeder.SeedAsync(db);
        await VehicleListingDemoSeeder.SeedAsync(db, users);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseGlobalExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();