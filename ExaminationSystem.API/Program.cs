using ExaminationSystem.API.Extensions;
using ExaminationSystem.Application;
using ExaminationSystem.Infrastructure;
using ExaminationSystem.Infrastructure.Data;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add all services and Mapster configuration
builder.Services
    .AddApplicationServices()
    .AddMapsterConfiguration();

// Add FluentValidation
builder.Services
    .AddFluentValidation();

// Add Infrastructure services registrations
builder.Services
    .AddInfrastructureServices()
    .AddSecurityConfiguration(builder.Configuration);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine(connectionString);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseSqlServer(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        .LogTo(log => Debug.WriteLine(log), LogLevel.Information)
        .EnableSensitiveDataLogging();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomCors(app.Configuration);

app.UseAuthorization();
app.MapControllers();

app.Run();
