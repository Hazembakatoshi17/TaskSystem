# 🗂️ TaskSystem — Workspace Management API

A **RESTful backend API** for managing organizational workspaces. Built with ASP.NET Core 10, Entity Framework Core, Oracle Database, and JWT authentication.

---

## 🚀 Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 |
| Database | Oracle Database (FREEPDB1) |
| Authentication | JWT Bearer Tokens |
| API Docs | Swagger / OpenAPI |
| Language | C# 13 / .NET 10 |

---

## 📦 NuGet Packages

```xml
Microsoft.AspNetCore.Authentication.JwtBearer  10.0.6
Microsoft.AspNetCore.OpenApi                   10.0.5
Microsoft.EntityFrameworkCore                  10.0.6
Microsoft.EntityFrameworkCore.Tools            10.0.6
Microsoft.IdentityModel.Tokens                 8.17.0
Oracle.EntityFrameworkCore                     10.23.26200
Oracle.ManagedDataAccess.Core                  23.26.200
Swashbuckle.AspNetCore                         10.1.7
System.IdentityModel.Tokens.Jwt                8.17.0
```

---

## ⚙️ Getting Started

### Prerequisites
- .NET 10 SDK
- Oracle Database (XE or higher) running on `localhost:1521`
- Oracle PDB: `FREEPDB1`

### 1. Clone & configure

Update `appsettings.json` with your Oracle credentials:

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=YOUR_USER;Password=YOUR_PASS;Data Source=localhost:1521/FREEPDB1;"
  },
  "JwtSettings": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "TaskSystem",
    "Audience": "TaskSystemUsers",
    "ExpireDays": 7
  }
}
```

### 2. Create the database tables

Run in Oracle SQL Developer:

```sql
CREATE TABLE Departments (
    Dept_Id       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Dept_Name     VARCHAR2(200) NOT NULL,
    Dept_Description VARCHAR2(1000)
);

CREATE TABLE Employees (
    Emp_Id            NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Emp_Fname         VARCHAR2(100) NOT NULL,
    Emp_Lname         VARCHAR2(100) NOT NULL,
    Emp_MobileNumber  VARCHAR2(20)  NOT NULL,
    Emp_Password      VARCHAR2(255) NOT NULL,
    IsAdmin           NUMBER(1) DEFAULT 0 NOT NULL,
    Dept_Id           NUMBER,
    CONSTRAINT FK_Emp_Dept FOREIGN KEY (Dept_Id) REFERENCES Departments(Dept_Id) ON DELETE SET NULL
);

CREATE TABLE Projects (
    Proj_Id          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Proj_Name        VARCHAR2(200) NOT NULL,
    Proj_Description VARCHAR2(1000),
    Proj_StartDate   DATE NOT NULL,
    Proj_EndDate     DATE
);

CREATE TABLE ProjectEmployees (
    Proj_Id    NUMBER NOT NULL,
    Emp_Id     NUMBER NOT NULL,
    JoinedDate DATE DEFAULT SYSDATE,
    CONSTRAINT PK_PE     PRIMARY KEY (Proj_Id, Emp_Id),
    CONSTRAINT FK_PE_Proj FOREIGN KEY (Proj_Id) REFERENCES Projects(Proj_Id),
    CONSTRAINT FK_PE_Emp  FOREIGN KEY (Emp_Id)  REFERENCES Employees(Emp_Id)
);

CREATE TABLE Tasks (
    Task_Id          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Task_Title       VARCHAR2(300) NOT NULL,
    Task_Description VARCHAR2(2000),
    Task_Status      VARCHAR2(20) DEFAULT 'Pending',
    Task_Priority    VARCHAR2(20) DEFAULT 'Medium',
    Task_DueDate     DATE NOT NULL,
    Proj_Id          NUMBER NOT NULL,
    CONSTRAINT FK_Task_Proj FOREIGN KEY (Proj_Id) REFERENCES Projects(Proj_Id) ON DELETE CASCADE
);

CREATE TABLE TaskAssignments (
    Assignment_Id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Task_Id       NUMBER NOT NULL,
    Emp_Id        NUMBER NOT NULL,
    AssignedDate  DATE DEFAULT SYSDATE,
    CONSTRAINT FK_TA_Task FOREIGN KEY (Task_Id) REFERENCES Tasks(Task_Id) ON DELETE CASCADE,
    CONSTRAINT FK_TA_Emp  FOREIGN KEY (Emp_Id)  REFERENCES Employees(Emp_Id)
);
```

### 3. Seed a department admin

```sql
-- Create a department
INSERT INTO Departments (Dept_Name, Dept_Description)
VALUES ('Engineering', 'Software development team');

-- Create an admin employee for that department
INSERT INTO Employees (Emp_Fname, Emp_Lname, Emp_MobileNumber, Emp_Password, IsAdmin, Dept_Id)
VALUES ('Ahmed', 'Al-Rashid', '0501234567', 'securepass123', 1, 1);
COMMIT;
```

### 4. Run the project

```bash
dotnet run --project TaskSystem/TaskSystem.csproj
```

Open your browser at `http://localhost:5000` — Swagger UI loads at the root.

---

## 🔐 Authentication

All protected endpoints require a `Bearer` JWT token in the `Authorization` header.

**Login:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "Emp_MobileNumber": "0501234567",
  "Emp_Password": "securepass123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "name": "Ahmed Al-Rashid",
  "mobile": "0501234567"
}
```

Use the token in Swagger by clicking **Authorize** and entering: `Bearer <your_token>`

---

## 👥 Roles & Permissions

| Action | Employee | Admin |
|---|---|---|
| Login | ✅ | ✅ |
| View own tasks | ✅ | — |
| Update task status | ✅ (own tasks only) | ✅ |
| View all dept tasks | ❌ | ✅ |
| Create / Edit / Delete tasks | ❌ | ✅ (own dept only) |
| Assign employees to tasks | ❌ | ✅ (same dept only) |
| Manage employees | ✅ (open) | ✅ |
| Manage departments | ✅ (open) | ✅ |
| Manage projects | ✅ (open) | ✅ |

---

## 📡 API Endpoints Summary

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | ❌ | Login, get JWT token |

### Employees
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/employee` | ❌ | Get all employees |
| GET | `/api/employee/{id}` | ❌ | Get employee by ID |
| POST | `/api/employee` | ❌ | Create employee |
| PUT | `/api/employee/{id}` | ❌ | Update employee |
| DELETE | `/api/employee/{id}` | ❌ | Delete employee |

### Departments
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/department` | ❌ | Get all departments |
| GET | `/api/department/{id}` | ❌ | Get department + its employees |
| POST | `/api/department` | ❌ | Create department |
| PUT | `/api/department/{id}` | ❌ | Update department |
| DELETE | `/api/department/{id}` | ❌ | Delete department |

### Projects
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/project` | ❌ | Get all projects |
| GET | `/api/project/{id}` | ❌ | Get project with members & tasks |
| POST | `/api/project` | ❌ | Create project |
| PUT | `/api/project/{id}` | ❌ | Update project |
| DELETE | `/api/project/{id}` | ❌ | Delete project |
| POST | `/api/project/{projId}/assign/{empId}` | ❌ | Add employee to project |
| DELETE | `/api/project/{projId}/remove/{empId}` | ❌ | Remove employee from project |

### Tasks
| Method | Endpoint | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/task` | ✅ | Any | Employee: own tasks. Admin: all dept tasks |
| GET | `/api/task/{id}` | ✅ | Any | Employee: only if assigned. Admin: dept scope |
| GET | `/api/task/project/{projId}` | ✅ | Admin | Tasks in a project |
| GET | `/api/task/status/{status}` | ✅ | Admin | Filter by Pending/InProgress/Done |
| GET | `/api/task/priority/{priority}` | ✅ | Admin | Filter by Low/Medium/High |
| POST | `/api/task` | ✅ | Admin | Create task (dept scope enforced) |
| PUT | `/api/task/{id}` | ✅ | Admin | Update task |
| PATCH | `/api/task/{id}/status` | ✅ | Any | Update status (employee: own tasks only) |
| DELETE | `/api/task/{id}` | ✅ | Admin | Delete task |

### Task Assignments
| Method | Endpoint | Auth | Role | Description |
|---|---|---|---|---|
| GET | `/api/taskassignment` | ✅ | Admin | All assignments in dept |
| GET | `/api/taskassignment/task/{taskId}` | ✅ | Admin | Assignments for a task |
| GET | `/api/taskassignment/employee/{empId}` | ✅ | Any | Employee: own. Admin: dept only |
| POST | `/api/taskassignment/assign` | ✅ | Admin | Assign employee to task (same dept enforced) |
| DELETE | `/api/taskassignment/{id}` | ✅ | Admin | Remove assignment by ID |
| DELETE | `/api/taskassignment/task/{taskId}/employee/{empId}` | ✅ | Admin | Remove by task+employee |

---

## 🏗️ Project Structure

```
TaskSystem/
├── Controllers/
│   ├── AuthController.cs          # Login → JWT
│   ├── EmployeeController.cs      # Employee CRUD
│   ├── DepartmentController.cs    # Department CRUD
│   ├── ProjectController.cs       # Project CRUD + member management
│   ├── TaskController.cs          # Task CRUD + role-based filtering
│   └── TaskAssignmentController.cs# Assign/unassign employees to tasks
├── Data/
│   └── AppDbContext.cs            # EF Core context + relationship config
├── Extensions/
│   └── ClaimsExtensions.cs        # JWT claim helpers (GetEmpId, GetDeptId, GetIsAdmin)
├── Models/
│   ├── Department.cs
│   ├── Employee.cs                # IsAdmin flag
│   ├── LoginRequest.cs
│   ├── Project.cs
│   ├── ProjectEmployee.cs         # Join table: composite PK
│   ├── TaskAssignment.cs
│   └── TaskItem.cs                # WorkTaskStatus + WorkTaskPriority enums
├── Services/
│   └── JwtService.cs              # Token generation with dept_id + role claims
├── appsettings.json
└── Program.cs                     # DI, JWT config, Swagger setup
```

---

## 🔗 Entity Relationships

```
Department  1 ──────────── * Employee
Employee    * ──────────── * Project   (via ProjectEmployee)
Project     1 ──────────── * TaskItem
TaskItem    * ──────────── * Employee  (via TaskAssignment)
```

**Business Rules:**
- Each employee belongs to one department (nullable)
- One employee per department holds the `IsAdmin = true` role
- Admins can only manage tasks and assign employees **within their own department**
- Employees can only view and update status of **tasks assigned to them**
- Task assignments are restricted to employees in the **same department** as the admin

---

## 🧩 JWT Token Claims

| Claim | Value | Used For |
|---|---|---|
| `id` | Employee ID | Identifying caller |
| `mobile` | Mobile number | Identity |
| `name` | Full name | Display |
| `dept_id` | Department ID | Scoping queries to department |
| `is_admin` | `"true"` / `"false"` | Logic branching |
| `ClaimTypes.Role` | `"Admin"` / `"Employee"` | `[Authorize(Policy="AdminOnly")]` |
