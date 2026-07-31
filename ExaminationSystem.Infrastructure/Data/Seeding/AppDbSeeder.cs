using Bogus;
using ExaminationSystem.Application.InfraInterfaces;
using ExaminationSystem.Application.Interfaces;
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
        var tenantAccessor = scope.ServiceProvider.GetRequiredService<ITenantAccessor>();
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

        // 0.1 Seed Tenant Domains
        var tenantDomains = new List<TenantDomain>
        {
            new TenantDomain { TenantId = defaultTenantId, Domain = "localhost", IsPrimary = true },
            new TenantDomain { TenantId = defaultTenantId, Domain = "defaultuniversity.example.com" },
            new TenantDomain { TenantId = secondTenantId, Domain = "techacademy.example.com", IsPrimary = true }
        };
        await context.TenantDomains.AddRangeAsync(tenantDomains);
        await context.SaveChangesAsync();

        // Set default tenant for seeding operations so the DbContext auto-assign works
        tenantAccessor.SetTenantId(defaultTenantId);

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

        // 2. Generate Random Extra Instructors (~6)
        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.Name))
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.Password, f => fixedPassword)
            .RuleFor(u => u.IsEmailConfirmed, f => true);

        var randomInstructorsUsers = userFaker.Clone()
            .RuleFor(u => u.Role, f => UserRole.Instructor)
            .RuleFor(u => u.TenantId, f => f.PickRandom(defaultTenantId, secondTenantId))
            .Generate(6);
        await context.AppUsers.AddRangeAsync(randomInstructorsUsers);
        await context.SaveChangesAsync();

        var randomInstructors = randomInstructorsUsers.Select(u => new Instructor { AppUser = u, TenantId = u.TenantId }).ToList();
        await context.Instructors.AddRangeAsync(randomInstructors);
        await context.SaveChangesAsync();

        // 3. Generate Random Extra Students (~30)
        var randomStudentUsers = userFaker.Clone()
            .RuleFor(u => u.Role, f => UserRole.Student)
            .RuleFor(u => u.TenantId, f => f.PickRandom(defaultTenantId, secondTenantId))
            .Generate(30);
        await context.AppUsers.AddRangeAsync(randomStudentUsers);
        await context.SaveChangesAsync();

        var randomStudents = randomStudentUsers.Select(u => new Student { AppUser = u, TenantId = u.TenantId }).ToList();
        await context.Students.AddRangeAsync(randomStudents);
        await context.SaveChangesAsync();

        // Combine all Instructors/Students
        var allInstructors = new List<Instructor> { adminInstructor }.Concat(randomInstructors).ToList();
        var allStudents = new List<Student> { fixedStudent }.Concat(randomStudents).ToList();

        // 4. Generate Courses (10) across the mapping of instructors
        var courseNames = new List<string>
        {
            "Introduction to Computer Science",
            "Data Structures & Algorithms",
            "Database Systems",
            "Software Engineering",
            "Calculus I",
            "Calculus II",
            "Linear Algebra",
            "General Chemistry",
            "Introduction to Physics",
            "Artificial Intelligence",
            "Machine Learning",
            "Computer Networks",
            "Operating Systems",
            "Cybersecurity Fundamentals",
            "Web Development"
        };
        var shuffledCourseNames = new Faker().Random.Shuffle(courseNames).ToList();
        int courseIndex = 0;

        var courseFaker = new Faker<Course>()
            .RuleFor(c => c.Title, f => shuffledCourseNames[courseIndex++ % shuffledCourseNames.Count])
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreditHours, f => f.Random.Int(1, 4))
            .RuleFor(c => c.MaxEnrollment, f => f.Random.Int(3, 10) * 10)
            .RuleFor(c => c.InstructorID, (f, c) => f.PickRandom(allInstructors).ID)
            .RuleFor(c => c.TenantId, (f, c) =>
            {
                var instructor = allInstructors.First(i => i.ID == c.InstructorID);
                return instructor.TenantId;
            });

        var courses = courseFaker.Generate(10);
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
            var courseExamCount = new Faker().Random.Int(3, 4);
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

        // Generate Questions for exams using realistic course-matched questions
        foreach (var exam in exams)
        {
            var course = courses.FirstOrDefault(c => c.ID == exam.CourseID);
            var courseTitle = course?.Title ?? "";

            // Find matching bank or fallback to Computer Science questions
            List<SampleQuestion> questionBank = CourseQuestionBank.FirstOrDefault(kvp => courseTitle.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)).Value
                ?? CourseQuestionBank["Data Structures & Algorithms"];

            var shuffledBank = new Faker().Random.Shuffle(questionBank).ToList();
            var examQuestionCount = Math.Min(shuffledBank.Count, new Faker().Random.Int(4, 6));
            var pointsPerQuestion = exam.TotalGrade / examQuestionCount;

            for (int q = 0; q < examQuestionCount; q++)
            {
                var sampleQ = shuffledBank[q];
                var question = new Question
                {
                    Body = sampleQ.Body,
                    Score = pointsPerQuestion,
                    QuestionLevel = new Faker().PickRandom<QuestionLevel>(),
                    TenantId = exam.TenantId
                };

                await context.Questions.AddAsync(question);
                await context.SaveChangesAsync(); // Need ID for choices

                // Generate Choices (shuffled choices with accurate correct flag)
                var choicesList = sampleQ.Choices.Select((body, idx) => new Choice
                {
                    QuestionId = question.ID,
                    Body = body,
                    IsCorrect = idx == sampleQ.CorrectIndex,
                    TenantId = exam.TenantId
                }).ToList();

                var shuffledChoices = new Faker().Random.Shuffle(choicesList).ToList();
                choices.AddRange(shuffledChoices);

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

        // 7. Simulate Past Exam Attempts (~60-80)
        var pastAttempts = new List<ExamAttempt>();
        var studentExamAnswers = new List<StudentExamsAnswers>();

        var attemptCount = new Faker().Random.Int(60, 80);
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

    private record SampleQuestion(string Body, string[] Choices, int CorrectIndex);

    private static readonly Dictionary<string, List<SampleQuestion>> CourseQuestionBank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Database Systems"] = new()
        {
            new("Which SQL clause is used to filter records after aggregation?", new[] { "HAVING", "WHERE", "GROUP BY", "ORDER BY" }, 0),
            new("What property of ACID guarantees that a transaction will be completed fully or not at all?", new[] { "Atomicity", "Consistency", "Isolation", "Durability" }, 0),
            new("Which normal form eliminates partial functional dependencies?", new[] { "Second Normal Form (2NF)", "First Normal Form (1NF)", "Third Normal Form (3NF)", "Boyce-Codd Normal Form" }, 0),
            new("Which database index structure is most commonly optimized for range queries?", new[] { "B+ Tree", "Hash Table", "Binary Search Tree", "Inverted Index" }, 0),
            new("What type of JOIN returns all records when there is a match in either left or right table?", new[] { "FULL OUTER JOIN", "INNER JOIN", "LEFT JOIN", "CROSS JOIN" }, 0),
            new("What keyword is used to eliminate duplicate records from a SQL query result?", new[] { "DISTINCT", "UNIQUE", "GROUP BY", "SINGLE" }, 0)
        },
        ["Data Structures & Algorithms"] = new()
        {
            new("What is the worst-case time complexity of Quick Sort?", new[] { "O(n²)", "O(n log n)", "O(n)", "O(log n)" }, 0),
            new("Which data structure operates on a Last-In, First-Out (LIFO) basis?", new[] { "Stack", "Queue", "Min Heap", "Linked List" }, 0),
            new("What algorithm finds the shortest path in a graph with non-negative edge weights?", new[] { "Dijkstra's Algorithm", "Bellman-Ford Algorithm", "Kruskal's Algorithm", "Depth-First Search" }, 0),
            new("What is the average time complexity for searching a key in a Hash Table?", new[] { "O(1)", "O(log n)", "O(n)", "O(n log n)" }, 0),
            new("Which traversal technique visits the root node first, then left subtree, then right subtree?", new[] { "Pre-order Traversal", "In-order Traversal", "Post-order Traversal", "Level-order Traversal" }, 0)
        },
        ["Software Engineering"] = new()
        {
            new("Which design pattern ensures a class has only one instance and provides a global access point?", new[] { "Singleton Pattern", "Factory Method Pattern", "Observer Pattern", "Strategy Pattern" }, 0),
            new("In Agile methodology, what is a fixed time period in which specific work must be completed?", new[] { "Sprint", "Backlog", "Milestone", "Roadmap" }, 0),
            new("Which testing phase verifies that individual software components or units work as intended?", new[] { "Unit Testing", "Integration Testing", "System Testing", "Acceptance Testing" }, 0),
            new("What principle states that software entities should be open for extension but closed for modification?", new[] { "Open-Closed Principle (OCP)", "Single Responsibility Principle (SRP)", "Liskov Substitution Principle (LSP)", "Dependency Inversion Principle (DIP)" }, 0)
        },
        ["Web Development"] = new()
        {
            new("Which HTTP method is idempotent and intended to completely update or replace a target resource?", new[] { "PUT", "POST", "PATCH", "DELETE" }, 0),
            new("Which security header helps prevent Cross-Site Scripting (XSS) attacks in modern web applications?", new[] { "Content-Security-Policy", "Access-Control-Allow-Origin", "Strict-Transport-Security", "X-Frame-Options" }, 0),
            new("What client-side technology allows asynchronous web communication without reloading the page?", new[] { "Fetch API / AJAX", "Cookies", "WebSockets", "Local Storage" }, 0),
            new("Which CSS layout model provides a one-dimensional layout method for aligning items in rows or columns?", new[] { "Flexbox", "CSS Grid", "Absolute Positioning", "Float" }, 0)
        },
        ["Computer Networks"] = new()
        {
            new("Which layer of the OSI model is responsible for routing packets across interconnected networks?", new[] { "Network Layer (Layer 3)", "Data Link Layer (Layer 2)", "Transport Layer (Layer 4)", "Application Layer (Layer 7)" }, 0),
            new("Which protocol translates human-readable domain names into IP addresses?", new[] { "DNS", "DHCP", "ARP", "NAT" }, 0),
            new("What reliable transport protocol guarantees ordered, error-checked delivery of stream data?", new[] { "TCP", "UDP", "ICMP", "IP" }, 0),
            new("Which port is the standard default for secure HTTP traffic (HTTPS)?", new[] { "443", "80", "22", "8080" }, 0)
        },
        ["Operating Systems"] = new()
        {
            new("Which CPU scheduling algorithm assigns fixed time slices to processes in cyclic order?", new[] { "Round Robin", "First-Come, First-Served", "Shortest Job First", "Priority Scheduling" }, 0),
            new("What condition occurs when two or more processes are permanently blocked waiting for resources held by each other?", new[] { "Deadlock", "Starvation", "Race Condition", "Thrashing" }, 0),
            new("What memory management technique allows an execution process to exceed physical memory limits?", new[] { "Virtual Memory", "Paging", "Segmentation", "Cache Memory" }, 0)
        },
        ["Artificial Intelligence"] = new()
        {
            new("Which machine learning paradigm trains models using labeled training data?", new[] { "Supervised Learning", "Unsupervised Learning", "Reinforcement Learning", "Self-Supervised Learning" }, 0),
            new("What activation function outputs values in the range (0, 1) and is commonly used for binary classification?", new[] { "Sigmoid", "ReLU", "Softmax", "Tanh" }, 0),
            new("What algorithm is widely used to calculate gradients and update weights in neural network training?", new[] { "Backpropagation", "Gradient Ascent", "K-Means Clustering", "Principal Component Analysis" }, 0)
        },
        ["Calculus I"] = new()
        {
            new("What is the derivative of f(x) = sin(x) with respect to x?", new[] { "cos(x)", "-cos(x)", "-sin(x)", "tan(x)" }, 0),
            new("What is the integral of 1/x dx for x > 0?", new[] { "ln(x) + C", "e^x + C", "-1/x² + C", "x² / 2 + C" }, 0),
            new("What theorem states that if a function is continuous on [a,b] and differentiable on (a,b), there exists c in (a,b) where f'(c) equals average rate of change?", new[] { "Mean Value Theorem", "Fundamental Theorem of Calculus", "Rolle's Theorem", "Intermediate Value Theorem" }, 0)
        }
    };
}
