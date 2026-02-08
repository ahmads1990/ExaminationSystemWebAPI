# 🎓 Examination System Web API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white)

**A modern, scalable examination management system built with Clean Architecture principles**

[Features](#-features) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [API Documentation](#-api-documentation) • [Tech Stack](#-tech-stack)

</div>

---

## 📖 Overview

The **Examination System Web API** is a comprehensive backend solution for managing academic examinations, courses, and student assessments. Built with **.NET 8** and following **Clean Architecture** principles, it provides a robust, scalable, and maintainable platform for educational institutions.

### 🎯 Key Highlights

- 🏗️ **Clean Architecture** - Clear separation of concerns across four distinct layers
- 🔐 **Secure Authentication** - JWT-based authentication with email verification
- 📧 **Email Integration** - Professional email templates for user communications
- ⚡ **High Performance** - Redis caching and optimized database queries
- 🔄 **Background Jobs** - Hangfire for asynchronous task processing
- ✅ **Robust Validation** - FluentValidation for comprehensive input validation
- 📚 **API Documentation** - Interactive Swagger/OpenAPI documentation
- 🧪 **Testable** - Unit tests with clear separation of concerns

---

## ✨ Features

### 👤 User Management
- **Role-Based Access** - Separate registration flows for Students and Instructors
- **Email Verification** - Secure token-based email confirmation
- **JWT Authentication** - Stateless, scalable authentication
- **Password Security** - Industry-standard password hashing

### 📚 Course Management
- **Course Creation** - Instructors can create and manage courses
- **Student Enrollment** - Students can enroll in available courses
- **Course Organization** - Structured academic content management

### 📝 Examination System
- **Question Bank** - Reusable questions with difficulty levels (Easy, Medium, Hard)
- **Flexible Exam Creation** - Instructors build exams from question bank
- **Multiple Choice Questions** - Support for multiple answer choices
- **Exam Configuration**
  - Time limits and duration control
  - Pass marks and grading criteria
  - Publish/Draft states
  - Deadline management
- **Automatic Grading** - Instant results based on correct answers
- **Exam Types** - Support for different examination formats

### 🎓 Student Features
- **Course Enrollment** - Browse and enroll in courses
- **Take Exams** - Timed examination interface
- **View Results** - Access grades and performance metrics

### 👨‍🏫 Instructor Features
- **Course Management** - Create and manage courses
- **Question Creation** - Build comprehensive question banks
- **Exam Design** - Create exams with custom configurations
- **Student Monitoring** - Track student performance

---

## 🏗️ Architecture

This project implements **Clean Architecture** with clear dependency rules:

```
┌─────────────────────────────────────────┐
│         ExaminationSystem.API           │  ← Presentation Layer
│     (Controllers, Models, Validators)   │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│      ExaminationSystem.Application      │  ← Business Logic Layer
│   (Services, DTOs, Interfaces, Mappers) │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│    ExaminationSystem.Infrastructure     │  ← Data Access Layer
│  (DbContext, Repositories, Services)    │
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│       ExaminationSystem.Domain          │  ← Core Domain Layer
│         (Entities, Interfaces)          │
└─────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Purpose | Dependencies |
|-------|---------|--------------|
| **Domain** | Core business entities and interfaces | None (pure C#) |
| **Application** | Business logic, services, DTOs | Domain |
| **Infrastructure** | Data access, external services | Application, Domain |
| **API** | HTTP endpoints, request/response models | Application, Infrastructure |

---

## 🛠️ Tech Stack

### Core Framework
- **.NET 8.0** - Latest LTS version with modern C# features
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core 8.0** - ORM for database operations

### Database & Caching
- **SQL Server** - Primary relational database
- **Redis** - Distributed caching for performance
- **Code-First Migrations** - Version-controlled database schema

### Security & Authentication
- **JWT Bearer Tokens** - Stateless authentication
- **Custom Authentication** - Flexible user management
- **Password Hashing** - Secure password storage
- **Email Verification** - Token-based email confirmation

### Background Processing
- **Hangfire** - Background job processing
- **Hangfire Dashboard** - Job monitoring UI at `/hangfire`

### Validation & Mapping
- **FluentValidation** - Declarative validation rules
- **Mapster** - High-performance object mapping

### Communication
- **MailKit** - Email sending with HTML templates
- **SMTP Integration** - Configurable email service

### Documentation & Tools
- **Swagger/OpenAPI** - Interactive API documentation
- **XML Documentation** - Code-level documentation

### 🛠️ API & Infrastructure Enhancements
- **Global Error Handling**: Middleware-based exception management for consistent error responses.
- **Automatic Transactions**: Middleware for automatic EF Core transactions on all write operations.
- **Response Compression**: Brotli and Gzip compression for optimized network usage.
- **Standardized Response Model**: Programmatic `ApiErrorCode` system with attribute-based messages.
- **FluentValidation Integration**: Automated request validation with localized error messages.

---

## 🎓 Lessons Learned & Best Practices

Throughout the development of these enhancements, several key architectural and technical lessons were implemented:

1.  **Middleware Ordering is Critical**: We learned that the order of middleware in `Program.cs` significantly impacts behavior. For instance, `UseResponseCompression` must come before error handling to ensure error responses are also compressed.
2.  **Clean Separation of Attributes**: Moving `ErrorMessageAttribute` to the **Application Layer** ensures that service-level enums can remain decoupled from the presentation layer (API) while still providing rich metadata for error reporting.
3.  **Pattern Matching vs. Static Properties**: Discovered that C# pattern matching (`is` with `or`) requires compile-time constants. Using `static readonly` properties like `HttpMethods.Get` requires traditional equality checks or helper methods.
4.  **Automatic Transaction Management**: Implementing transaction logic at the middleware level reduces boilerplate in services and ensures that business logic stays focused on the domain rather than infrastructure concerns.
5.  **Performance-First Error Handling**: Leveraging cached reflection for attribute lookups in the `EnumExtensions` ensures that retrieving error messages has minimal performance impact on the request pipeline.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or full version)
- [Redis](https://redis.io/download) (optional for development)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/ExaminationSystemWebAPI.git
   cd ExaminationSystemWebAPI
   ```

2. **Configure application settings**
   ```bash
   cd ExaminationSystem.API
   cp appsettings.json.example appsettings.json
   ```

3. **Update `appsettings.json`** with your configuration:
   - Database connection string
   - JWT settings (Key, Issuer, Audience)
   - SMTP email configuration
   - Redis connection (if using)

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project ExaminationSystem.Infrastructure --startup-project ExaminationSystem.API
   ```

5. **Run the application**
   ```bash
   dotnet run --project ExaminationSystem.API
   ```

6. **Access Swagger UI**
   - Navigate to: `https://localhost:5001/swagger`
   - Or: `http://localhost:5000/swagger`

---

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/RegisterInstructor` | Register a new instructor |
| POST | `/api/Auth/RegisterStudent` | Register a new student |
| POST | `/api/Auth/Login` | Authenticate and receive JWT token |
| POST | `/api/Auth/VerifyEmail` | Verify email with token |
| POST | `/api/Auth/ResendVerificationEmail` | Resend verification email |

### Course Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Courses` | Get all courses (paginated) |
| GET | `/api/Courses/{id}` | Get course by ID |
| POST | `/api/Courses` | Create new course (Instructor) |
| PUT | `/api/Courses/{id}` | Update course (Instructor) |
| DELETE | `/api/Courses/{id}` | Delete course (Instructor) |

### Exam Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Exams` | Get all exams |
| GET | `/api/Exams/{id}` | Get exam by ID |
| POST | `/api/Exams` | Create new exam (Instructor) |
| PUT | `/api/Exams/{id}` | Update exam (Instructor) |
| DELETE | `/api/Exams/{id}` | Delete exam (Instructor) |

### Question Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Questions` | Get all questions (paginated) |
| GET | `/api/Questions/{id}` | Get question by ID |
| POST | `/api/Questions` | Create new question (Instructor) |
| PUT | `/api/Questions/{id}` | Update question (Instructor) |
| DELETE | `/api/Questions/{id}` | Delete question (Instructor) |

### Student Course Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/StudentCourses` | Get student enrollments |
| POST | `/api/StudentCourses/Enroll` | Enroll in course (Student) |
| DELETE | `/api/StudentCourses/{id}` | Unenroll from course (Student) |

> 💡 **Tip:** For complete API documentation with request/response schemas, visit the Swagger UI when the application is running.

---

## 🔐 Authentication

This API uses **JWT Bearer Token** authentication.

### How to Authenticate

1. **Register** a new account (Student or Instructor)
2. **Verify** your email using the token sent to your inbox
3. **Login** with your credentials to receive a JWT token
4. **Include** the token in subsequent requests:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

### Swagger Authentication

1. Click the **Authorize** button in Swagger UI
2. Enter: `Bearer <your-token>`
3. Click **Authorize**
4. All requests will now include the token

---

## 🗄️ Database Schema

### Core Entities

- **AppUser** - Base user entity with authentication data
- **Student** - Student-specific information
- **Instructor** - Instructor-specific information
- **Course** - Academic courses
- **Exam** - Examinations with configuration
- **Question** - Question bank with difficulty levels
- **Choice** - Answer options for questions
- **ExamQuestion** - Many-to-many relationship (Exam ↔ Question)
- **StudentCourses** - Many-to-many relationship (Student ↔ Course)
- **StudentExamsAnswers** - Student exam submissions and answers

---

## ⚙️ Configuration

### Required Settings

#### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ExaminationSystemDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

#### JWT
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "ExaminationSystemAPI",
    "Audience": "ExaminationSystemClients",
    "DurationInHours": 24
  }
}
```

#### SMTP (Email)
```json
{
  "SMTPConfig": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@examsystem.com",
    "FromName": "Examination System"
  }
}
```

#### Redis (Optional)
```json
{
  "RedisConfig": {
    "Host": "localhost",
    "Port": 6379,
    "User": "",
    "Password": "",
    "Ssl": false,
    "AbortOnConnectFail": false,
    "InstanceName": "ExamSystem_"
  }
}
```

---

## 🧪 Testing

Run unit tests:
```bash
dotnet test ExaminationSystem.UnitTests
```

---

## 📦 Project Structure

```
ExaminationSystemWebAPI/
├── ExaminationSystem.API/              # Presentation Layer
│   ├── Controllers/                    # API Controllers
│   ├── Models/                         # Request/Response DTOs
│   ├── Validators/                     # FluentValidation validators
│   ├── Extensions/                     # Extension methods
│   └── Program.cs                      # Application entry point
│
├── ExaminationSystem.Application/      # Business Logic Layer
│   ├── Services/                       # Service implementations
│   ├── Interfaces/                     # Service interfaces
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Mappings/                      # Mapster configurations
│   └── EmailTemplates/                # Email HTML templates
│
├── ExaminationSystem.Infrastructure/   # Data Access Layer
│   ├── Data/                          # DbContext & Configurations
│   │   ├── AppDbContext.cs
│   │   ├── Config/                    # Entity configurations
│   │   ├── Migrations/                # EF Core migrations
│   │   └── Repositories/              # Repository implementations
│   ├── Services/                      # Infrastructure services
│   │   ├── Auth/                      # Token & Password helpers
│   │   └── Email/                     # Email service
│   └── Jobs/                          # Hangfire background jobs
│
├── ExaminationSystem.Domain/           # Core Domain Layer
│   ├── Entities/                      # Domain entities
│   ├── Interfaces/                    # Domain interfaces
│   └── Common/                        # Shared domain logic
│
├── ExaminationSystem.UnitTests/        # Unit Tests
│
└── agent/                              # AI Assistant Resources
    └── PROJECT_OVERVIEW.md            # Detailed project analysis
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Ahmad**

---

## 🙏 Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- Clean Architecture pattern by [Robert C. Martin](https://blog.cleancoder.com/)
- Inspired by modern educational technology needs

---

<div align="center">

**⭐ Star this repository if you find it helpful!**

Made with ❤️ using .NET 8

</div>