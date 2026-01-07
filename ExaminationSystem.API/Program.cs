using ExaminationSystem.API.Extensions;
using ExaminationSystem.Application;
using ExaminationSystem.Infrastructure;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
