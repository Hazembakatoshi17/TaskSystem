# 📋 TaskSystem API

A RESTful Web API built with **ASP.NET Core (.NET 10)** for managing employees, departments, projects, and task assignments. Uses **Oracle Database** for persistence and **JWT** for authentication.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Language | C# |
| ORM | Entity Framework Core |
| Database | Oracle DB (FREEPDB1) |
| Auth | JWT (HMAC-SHA256) |
| API Docs | Swagger / OpenAPI |

---

## 📁 Project Structure

```
TaskSystem/
├── Controllers/
│   ├── AuthController.cs
│   ├── DepartmentController.cs
│   ├── EmployeeController.cs
│   ├── ProjectController.cs
│   ├── TaskController.cs
│   └── TaskAssignmentController.cs
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Department.cs
│   ├── Employee.cs
│   ├── LoginRequest.cs
│   ├── Project.cs
│   ├── ProjectEmployee.cs
│   ├── TaskAssignment.cs
│   └── TaskItem.cs
├── Services/
│   └── JwtService.cs
├── appsettings.json
└── Program.cs
```

---

## ⚙️ Configuration

Update `appsettings.json` before running:

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=localhost:1521/FREEPDB1;"
  },
  "JwtSettings": {
    "Key": "your-secret-key-here",
    "Issuer": "TaskSystem",
    "Audience": "TaskSystemUsers",
    "ExpireDays": 7
  }
}
```

---

## ▶️ Running the Project

**Prerequisites:**
- .NET 10 SDK
- Oracle Database running locally on port `1521`

**Run:**
```bash
dotnet run --project TaskSystem/TaskSystem
```

Swagger UI will be available at: `https://localhost:{port}/`

---

## 🔐 Authentication

Login with mobile number and password to receive a JWT token:

```http
POST /api/Auth/login
Content-Type: application/json

{
  "Emp_MobileNumber": "05xxxxxxxx",
  "Emp_Password": "yourpassword"
}
```

**Response:**
```json
{
  "token": "<JWT>",
  "name": "John Doe",
  "mobile": "05xxxxxxxx"
}
```

Use the token in subsequent requests:
```
Authorization: Bearer <token>
```

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Auth/login` | Login and get JWT token |

### Employees
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Employee` | Get all employees |
| GET | `/api/Employee/{id}` | Get employee by ID |
| POST | `/api/Employee` | Create employee |
| PUT | `/api/Employee/{id}` | Update employee |
| DELETE | `/api/Employee/{id}` | Delete employee |

### Departments
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Department` | Get all departments |
| GET | `/api/Department/{id}` | Get department by ID |
| POST | `/api/Department` | Create department |
| PUT | `/api/Department/{id}` | Update department |
| DELETE | `/api/Department/{id}` | Delete department |

### Projects
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Project` | Get all projects |
| GET | `/api/Project/{id}` | Get project by ID |
| POST | `/api/Project` | Create project |
| PUT | `/api/Project/{id}` | Update project |
| DELETE | `/api/Project/{id}` | Delete project |
| POST | `/api/Project/{projId}/assign/{empId}` | Assign employee to project |
| DELETE | `/api/Project/{projId}/remove/{empId}` | Remove employee from project |

### Tasks
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/Task` | Get all tasks |
| GET | `/api/Task/{id}` | Get task by ID |
| GET | `/api/Task/project/{projId}` | Get tasks by project |
| GET | `/api/Task/status/{status}` | Filter by status (`Pending`, `InProgress`, `Done`) |
| GET | `/api/Task/priority/{priority}` | Filter by priority (`Low`, `Medium`, `High`) |
| POST | `/api/Task` | Create task |
| PUT | `/api/Task/{id}` | Update task |
| PATCH | `/api/Task/{id}/status` | Update task status only |
| DELETE | `/api/Task/{id}` | Delete task |

### Task Assignments
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/TaskAssignment` | Get all assignments |
| GET | `/api/TaskAssignment/task/{taskId}` | Get employees assigned to a task |
| GET | `/api/TaskAssignment/employee/{empId}` | Get tasks assigned to an employee |
| POST | `/api/TaskAssignment/assign` | Assign employee to task |
| DELETE | `/api/TaskAssignment/{assignmentId}` | Remove assignment by ID |
| DELETE | `/api/TaskAssignment/task/{taskId}/employee/{empId}` | Remove assignment by task + employee |

---

## 🗄️ Data Models

### TaskItem — Enums

```
Task_Status:   Pending | InProgress | Done
Task_Priority: Low | Medium | High
```

### Entity Relationships

```
Department  ──< Employee
Project     ──< ProjectEmployee >── Employee
Project     ──< TaskItem
TaskItem    ──< TaskAssignment >── Employee
```

---

## ⚠️ Security Notes (Pre-Production Checklist)

- [ ] **Hash passwords** — currently stored as plain text; use BCrypt or `PasswordHasher`
- [ ] **Enable `[Authorize]`** — commented out on `EmployeeController`; apply across all sensitive endpoints
- [ ] **Secure JWT secret** — move signing key out of `appsettings.json` into environment variables or a secrets manager
- [ ] **Disable Swagger in production** — currently enabled unconditionally
- [ ] **Secure DB credentials** — do not commit connection strings with real passwords to source control

---

## 📄 License

This project is for internal / academic use.
