using ExaminationSystem.API.Extensions;
using ExaminationSystem.Application;
using ExaminationSystem.Infrastructure;
using FluentValidation.AspNetCore;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure services registrations
builder.Services
    .AddInfrastructureServices()
    .AddInfraStructureConfiguration(builder.Configuration);

// Add all services and Mapster configuration
builder.Services
    .AddApplicationServices()
    .AddMapsterConfiguration();

// Add FluentValidation
builder.Services
    .AddFluentValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCustomCors(app.Configuration);

app.UseStaticFiles();
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();
app.MapControllers();

app.Run();
