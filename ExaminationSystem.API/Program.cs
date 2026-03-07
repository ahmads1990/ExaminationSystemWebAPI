using ExaminationSystem.API.Authorization;
using ExaminationSystem.API.Extensions;
using ExaminationSystem.API.Middlewares;
using ExaminationSystem.Application;
using ExaminationSystem.Application.Common;
using ExaminationSystem.Infrastructure;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
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
builder.Services.AddSwaggerConfiguration();

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

// Add Authorization Policies
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, ScopeHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.ExamAnswer,
        policy => policy.Requirements.Add(new ScopeRequirement(ScopeNames.ExamAnswer)));
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseResponseCompression();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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
app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseMiddleware<TransactionMiddleware>();

app.MapControllers();

app.Run();
