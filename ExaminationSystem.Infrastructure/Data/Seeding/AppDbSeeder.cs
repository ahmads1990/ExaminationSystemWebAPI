using Bogus;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExaminationSystem.Infrastructure.Data.Seeding;

public static class AppDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHelper = scope.ServiceProvider.GetRequiredService<IPasswordHelper>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Pre-check if DB is already seeded
        if (await context.AppUsers.IgnoreQueryFilters().AnyAsync())
        {
            logger.LogInformation("Database already contains data. Seeding skipped.");
            return;
        }

        logger.LogInformation("Attempting to seed database with Bogus...");

        // 0. Seed Tenants
        var tenants = new List<Tenant>
        {
            new Tenant { Name = "Default University", IsActive = true },
            new Tenant { Name = "Tech Academy", IsActive = true }
        };
        await context.Tenants.AddRangeAsync(tenants);
        await context.SaveChangesAsync();

        var defaultTenantId = tenants[0].ID;
        var secondTenantId = tenants[1].ID;

        // 1. Ensure Fixed Accounts Exist Always
        var fixedPassword = passwordHelper.HashPassword("Password123!");

        var adminUser = new AppUser
        {
            Name = "System Admin",
            Username = "admin",
            Email = "admin@exam.com",
            Password = fixedPassword,
            Role = UserRole.Instructor,
            IsEmailConfirmed = true,
            TenantId = defaultTenantId
        };

        var fixedStudentUser = new AppUser
        {
            Name = "System Student",
            Username = "student",
            Email = "student@exam.com",
            Password = fixedPassword,
            Role = UserRole.Student,
            IsEmailConfirmed = true,
            TenantId = defaultTenantId
        };

        await context.AppUsers.AddRangeAsync(adminUser, fixedStudentUser);
        await context.SaveChangesAsync(); // Get IDs

        var adminInstructor = new Instructor { AppUser = adminUser, TenantId = defaultTenantId };
        var fixedStudent = new Student { AppUser = fixedStudentUser, TenantId = defaultTenantId };

        await context.Instructors.AddAsync(adminInstructor);
        await context.Students.AddAsync(fixedStudent);
        await context.SaveChangesAsync();

        // 2. Generate Random Extra Instructors (~3)
        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.Name))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.Password, f => fixedPassword)
            .RuleFor(u => u.IsEmailConfirmed, f => true);

        var randomInstructorsUsers = userFaker.Clone()
            .RuleFor(u => u.Role, f => UserRole.Instructor)
            .RuleFor(u => u.TenantId, f => f.PickRandom(defaultTenantId, secondTenantId))
            .Generate(3);
        await context.AppUsers.AddRangeAsync(randomInstructorsUsers);
        await context.SaveChangesAsync();

        var randomInstructors = randomInstructorsUsers.Select(u => new Instructor { AppUser = u, TenantId = u.TenantId }).ToList();
        await context.Instructors.AddRangeAsync(randomInstructors);
        await context.SaveChangesAsync();

        // 3. Generate Random Extra Students (~10)
        var randomStudentUsers = userFaker.Clone()
            .RuleFor(u => u.Role, f => UserRole.Student)
            .RuleFor(u => u.TenantId, f => f.PickRandom(defaultTenantId, secondTenantId))
            .Generate(10);
        await context.AppUsers.AddRangeAsync(randomStudentUsers);
        await context.SaveChangesAsync();

        var randomStudents = randomStudentUsers.Select(u => new Student { AppUser = u, TenantId = u.TenantId }).ToList();
        await context.Students.AddRangeAsync(randomStudents);
        await context.SaveChangesAsync();

        // Combine all Instructors/Students
        var allInstructors = new List<Instructor> { adminInstructor }.Concat(randomInstructors).ToList();
        var allStudents = new List<Student> { fixedStudent }.Concat(randomStudents).ToList();

        // 4. Generate Courses (4-5) across the mapping of instructors
        var courseFaker = new Faker<Course>()
            .RuleFor(c => c.Title, f => f.Company.CatchPhrase())
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreditHours, f => f.Random.Int(1, 4))
            .RuleFor(c => c.InstructorID, (f, c) => f.PickRandom(allInstructors).ID)
            .RuleFor(c => c.TenantId, (f, c) =>
            {
                var instructor = allInstructors.First(i => i.ID == c.InstructorID);
                return instructor.TenantId;
            });

        var courses = courseFaker.Generate(5);
        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();

        // 5. Enroll Students into Courses (only same-tenant students into same-tenant courses)
        var studentCourses = new List<StudentCourses>();
        foreach (var student in allStudents)
        {
            var sameTenantCourses = courses.Where(c => c.TenantId == student.TenantId).ToList();
            if (!sameTenantCourses.Any()) continue;

            var selectedCourses = new Faker().PickRandom(sameTenantCourses, Math.Min(new Faker().Random.Int(2, 3), sameTenantCourses.Count)).ToList();
            foreach (var course in selectedCourses)
            {
                studentCourses.Add(new StudentCourses
                {
                    StudentID = student.ID,
                    CourseID = course.ID,
                    EnrollmentDate = DateTime.UtcNow,
                    Finished = false,
                    TenantId = student.TenantId
                });
            }
        }
        await context.Set<StudentCourses>().AddRangeAsync(studentCourses);
        await context.SaveChangesAsync();

        // 6. Generate Exams & Questions
        var exams = new List<Exam>();
        var questions = new List<Question>();
        var choices = new List<Choice>();
        var examQuestions = new List<ExamQuestion>();

        foreach (var course in courses)
        {
            var courseExamCount = new Faker().Random.Int(1, 2);
            for (int e = 0; e < courseExamCount; e++)
            {
                var exam = new Exam
                {
                    CourseID = course.ID,
                    Title = $"{course.Title} - Exam {e + 1}",
                    ExamType = new Faker().PickRandom<ExamType>(),
                    MaxDurationInMinutes = new Faker().Random.Int(30, 120),
                    TotalGrade = 100,
                    PassingScore = 50,
                    MaxAttempts = new Faker().Random.Int(1, 3),
                    ShuffleQuestions = true,
                    ExamStatus = ExamStatus.Published, // Make it ready
                    PublishDate = DateTime.UtcNow.AddDays(-1),
                    DeadlineDate = DateTime.UtcNow.AddMonths(1),
                    TenantId = course.TenantId
                };
                exams.Add(exam);
            }
        }
        await context.Exams.AddRangeAsync(exams);
        await context.SaveChangesAsync();

        // Generate Questions for exams
        foreach (var exam in exams)
        {
            var examQuestionCount = new Faker().Random.Int(5, 10);
            var pointsPerQuestion = exam.TotalGrade / examQuestionCount;

            for (int q = 0; q < examQuestionCount; q++)
            {
                var question = new Question
                {
                    Body = new Faker().Lorem.Sentence() + "?",
                    Score = pointsPerQuestion,
                    QuestionLevel = new Faker().PickRandom<QuestionLevel>(),
                    TenantId = exam.TenantId
                };

                await context.Questions.AddAsync(question);
                await context.SaveChangesAsync(); // Need ID for choices

                // Generate Choices (4 choices, 1 correct)
                var correctIndex = new Faker().Random.Int(0, 3);
                for (int c = 0; c < 4; c++)
                {
                    var choice = new Choice
                    {
                        QuestionId = question.ID,
                        Body = new Faker().Lorem.Word(),
                        IsCorrect = c == correctIndex,
                        TenantId = exam.TenantId
                    };
                    choices.Add(choice);
                }

                examQuestions.Add(new ExamQuestion
                {
                    ExamId = exam.ID,
                    Exam = exam,
                    QuestionId = question.ID,
                    Question = question,
                    TenantId = exam.TenantId
                });
            }
        }
        await context.Choices.AddRangeAsync(choices);
        await context.ExamQuestions.AddRangeAsync(examQuestions);
        await context.SaveChangesAsync();

        // 7. Simulate Past Exam Attempts (~15-20)
        var pastAttempts = new List<ExamAttempt>();
        var studentExamAnswers = new List<StudentExamsAnswers>();

        var attemptCount = new Faker().Random.Int(15, 25);
        for (int i = 0; i < attemptCount; i++)
        {
            var student = new Faker().PickRandom(allStudents);
            var studentEnrolledCourseIds = studentCourses.Where(sc => sc.StudentID == student.ID).Select(sc => sc.CourseID);
            var validExamsForStudent = exams.Where(e => studentEnrolledCourseIds.Contains(e.CourseID)).ToList();

            if (!validExamsForStudent.Any()) continue;

            var exam = new Faker().PickRandom(validExamsForStudent);

            var attempt = new ExamAttempt
            {
                ExamId = exam.ID,
                StudentId = student.ID,
                StartTime = DateTime.UtcNow.AddDays(-new Faker().Random.Int(1, 10)),
                ExamAttemptStatus = ExamAttemptStatus.Graded,
                TenantId = student.TenantId
            };

            // Assign end time based on random duration
            attempt.EndTime = attempt.StartTime.AddMinutes(new Faker().Random.Int(10, exam.MaxDurationInMinutes));

            // Calculate a score
            var attemptQuestions = examQuestions.Where(eq => eq.ExamId == exam.ID).ToList();
            int totalScoreAchieved = 0;

            foreach (var examQ in attemptQuestions)
            {
                var questionChoices = choices.Where(c => c.QuestionId == examQ.QuestionId).ToList();
                // Pick a random choice, maybe right, maybe wrong
                var pickedChoice = new Faker().PickRandom(questionChoices);
                var isCorrect = pickedChoice.IsCorrect;

                if (isCorrect)
                {
                    var questScore = context.Questions.Local.FirstOrDefault(q => q.ID == examQ.QuestionId)?.Score ?? 0;
                    totalScoreAchieved += questScore;
                }

                studentExamAnswers.Add(new StudentExamsAnswers
                {
                    // We will set ExamAttemptID after saving attempts
                    ExamAttempt = attempt,
                    QuestionID = examQ.QuestionId,
                    ChoiceID = pickedChoice.ID,
                    StudentID = student.ID,
                    TenantId = student.TenantId
                });
            }

            attempt.Score = totalScoreAchieved;
            pastAttempts.Add(attempt);
        }

        await context.ExamAttempts.AddRangeAsync(pastAttempts);
        await context.Set<StudentExamsAnswers>().AddRangeAsync(studentExamAnswers);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeding completed successfully.");
    }
}

