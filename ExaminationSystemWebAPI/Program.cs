using ExaminationSystemWebAPI.Data;
using ExaminationSystemWebAPI.Data.GenericRepo;
using ExaminationSystemWebAPI.Services.ChoiceService;
using ExaminationSystemWebAPI.Services.ExamService;
using ExaminationSystemWebAPI.Services.QuestionService;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IChoiceService, ChoiceService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
    .UseSqlServer(connectionString)
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
    .LogTo(log => Debug.WriteLine(log), LogLevel.Information)
    .EnableSensitiveDataLogging();
});

// Mapster
// Tell Mapster to scan this assambly searching for the Mapster.IRegister classes and execute them
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
