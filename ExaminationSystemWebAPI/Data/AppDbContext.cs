﻿using ExaminationSystemWebAPI.Models;
using ExaminationSystemWebAPI.Models.Users;
using ExaminationSystemWebAPI.Models.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ExaminationSystemWebAPI.Data.Config;

namespace ExaminationSystemWebAPI.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Student> Students { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureChoice();
        modelBuilder.ConfigureExam();

        // Exam <-> Questions
        modelBuilder.ConfigureExamQusetions();
    }
}
