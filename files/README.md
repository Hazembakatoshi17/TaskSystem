# 📋 TaskSystem API

A RESTful Web API for managing employees, departments, projects, and task assignments — built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **Oracle Database**, secured with **JWT authentication**.

---

## 📌 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Authentication](#authentication)
- [Roles & Permissions](#roles--permissions)
- [API Reference](#api-reference)
  - [Auth](#auth)
  - [Departments](#departments)
  - [Employees](#employees)
  - [Projects](#projects)
  - [Tasks](#tasks)
  - [Task Assignments](#task-assignments)
- [Database Schema](#database-schema)
- [Business Rules](#business-rules)

---

## Overview

TaskSystem is a department-scoped task management backend. Admins manage projects and tasks within their department; employees view and update only the tasks assigned to them.

---

## Tech Stack

| Layer         | Technology                              |
|---------------|-----------------------------------------|
| Framework     | ASP.NET Core Web API (.NET 10)          |
| ORM           | Entity Framework Core 10                |
| Database      | Oracle (via Oracle.EntityFrameworkCore) |
| Auth          | JWT Bearer Tokens                       |
| API Docs      | Swagger / OpenAPI (Swashbuckle 10)      |
| Token Library | Microsoft.IdentityModel / System.IdentityModel.Tokens.Jwt |

---

## Project Structure

```
TaskSystem/
├── Controllers/
│   ├── AuthController.cs           # Login → JWT token
│   ├── DepartmentController.cs     # CRUD for departments
│   ├── EmployeeController.cs       # CRUD for employees
│   ├── ProjectController.cs        # CRUD + employee assignment
│   ├── TaskController.cs           # CRUD + status filter (role-aware)
│   └── TaskAssignmentController.cs # Assign/unassign employees to tasks
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext + model configuration
├── Extensions/
│   └── ClaimsExtensions.cs         # JWT claim helpers (GetEmpId, GetDeptId, GetIsAdmin)
├── Models/
│   ├── Department.cs
│   ├── Employee.cs
│   ├── LoginRequest.cs
│   ├── Project.cs
│   ├── ProjectEmployee.cs          # Join table: Project ↔ Employee
│   ├── TaskAssignment.cs           # Join table: Task ↔ Employee
│   └── TaskItem.cs                 # Enums: WorkTaskStatus, WorkTaskPriority
├── Services/
│   └── JwtService.cs               # Token generation
├── appsettings.json
└── Program.cs                      # App bootstrap, DI, middleware pipeline
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Oracle Database (e.g., Oracle Free — `FREEPDB1`)
- Oracle SQL Developer or SQL*Plus (to run migrations)

### 1. Clone & restore

```bash
git clone <repo-url>
cd TaskSystem/TaskSystem
dotnet restore
```

### 2. Configure the database

Edit `appsettings.json`:

```json
"ConnectionStrings": {
  "OracleDb": "User Id=<user>;Password=<pass>;Data Source=localhost:1521/FREEPDB1;"
}
```

### 3. Apply EF migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run

```bash
dotnet run
```

The API starts at `https://localhost:7xxx` and Swagger UI is available at the root path (`/`).

---

## Configuration

All settings live in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=system;Password=2030;Data Source=localhost:1521/FREEPDB1;"
  },
  "JwtSettings": {
    "Key":        "18d3bae7-a778-47ab-be0b-cce7ceaece15",  // ≥32 chars recommended
    "Issuer":     "TaskSystem",
    "Audience":   "TaskSystemUsers",
    "ExpireDays": 7
  }
}
```

> ⚠️ Replace the JWT key with a strong secret before deploying to production.

---

## Authentication

The API uses **JWT Bearer tokens**.

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "Emp_MobileNumber": "0501234567",
  "Emp_Password": "yourpassword"
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "name": "Ahmed Ali",
  "mobile": "0501234567"
}
```

### Using the token

Add the token to every protected request:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

In **Swagger UI**: click **Authorize** → enter `Bearer <token>`.

### Token claims

| Claim        | Value                          |
|--------------|--------------------------------|
| `id`         | Employee ID                    |
| `mobile`     | Mobile number                  |
| `name`       | Full name                      |
| `dept_id`    | Department ID                  |
| `is_admin`   | `"true"` or `"false"`          |
| `role`       | `"Admin"` or `"Employee"`      |

---

## Roles & Permissions

| Endpoint Area         | Employee | Admin |
|-----------------------|----------|-------|
| Login                 | ✅        | ✅     |
| View departments      | ✅        | ✅     |
| Manage departments    | ❌        | ✅     |
| View employees        | ✅        | ✅     |
| Manage employees      | ❌        | ✅     |
| View own tasks        | ✅        | —     |
| View dept tasks       | —        | ✅     |
| Create/edit tasks     | ❌        | ✅     |
| Update task status    | ✅ (own)  | ✅     |
| Assign employees      | ❌        | ✅ (dept only) |

**Department scoping:** Admins can only manage tasks, assignments, and employees within their own department.

---

## API Reference

### Auth

| Method | Route            | Auth | Description       |
|--------|------------------|------|-------------------|
| POST   | `/api/auth/login` | ❌   | Login, returns JWT |

---

### Departments

| Method | Route                  | Auth | Description              |
|--------|------------------------|------|--------------------------|
| GET    | `/api/department`       | ❌   | Get all departments       |
| GET    | `/api/department/{id}`  | ❌   | Get department by ID      |
| POST   | `/api/department`       | ❌   | Create department         |
| PUT    | `/api/department/{id}`  | ❌   | Update department         |
| DELETE | `/api/department/{id}`  | ❌   | Delete department         |

**Department object:**
```json
{
  "dept_Name": "Engineering",
  "dept_Description": "Software development team"
}
```

---

### Employees

| Method | Route                  | Auth | Description          |
|--------|------------------------|------|----------------------|
| GET    | `/api/employee`         | ❌   | Get all employees     |
| GET    | `/api/employee/{id}`    | ❌   | Get employee by ID    |
| POST   | `/api/employee`         | ❌   | Create employee       |
| PUT    | `/api/employee/{id}`    | ❌   | Update employee       |
| DELETE | `/api/employee/{id}`    | ❌   | Delete employee       |

**Employee object:**
```json
{
  "emp_Fname": "Ahmed",
  "emp_Lname": "Ali",
  "emp_MobileNumber": "0501234567",
  "emp_Password": "secret123",
  "isAdmin": false,
  "dept_Id": 1
}
```

---

### Projects

| Method | Route                                    | Auth | Description                     |
|--------|------------------------------------------|------|---------------------------------|
| GET    | `/api/project`                           | ❌   | Get all projects (with employees & tasks) |
| GET    | `/api/project/{id}`                      | ❌   | Get project by ID               |
| POST   | `/api/project`                           | ❌   | Create project                  |
| PUT    | `/api/project/{id}`                      | ❌   | Update project                  |
| DELETE | `/api/project/{id}`                      | ❌   | Delete project                  |
| POST   | `/api/project/{projId}/assign/{empId}`   | ❌   | Assign employee to project      |
| DELETE | `/api/project/{projId}/remove/{empId}`   | ❌   | Remove employee from project    |

**Project object:**
```json
{
  "proj_Name": "Website Redesign",
  "proj_Description": "Revamp the company portal",
  "proj_StartDate": "2025-01-01T00:00:00",
  "proj_EndDate": "2025-06-30T00:00:00"
}
```

---

### Tasks

> All task endpoints require a valid JWT (`Authorization: Bearer <token>`).

| Method | Route                          | Role     | Description                              |
|--------|--------------------------------|----------|------------------------------------------|
| GET    | `/api/task`                    | Any      | Employee: own tasks; Admin: dept tasks   |
| GET    | `/api/task/{id}`               | Any      | Get task (scoped by role)                |
| GET    | `/api/task/project/{projId}`   | Admin    | Get all tasks in a project               |
| GET    | `/api/task/status/{status}`    | Admin    | Filter tasks by status                   |
| GET    | `/api/task/priority/{priority}`| Admin    | Filter tasks by priority                 |
| POST   | `/api/task`                    | Admin    | Create task                              |
| PUT    | `/api/task/{id}`               | Admin    | Update task                              |
| PATCH  | `/api/task/{id}/status`        | Any      | Update status (employee: own tasks only) |
| DELETE | `/api/task/{id}`               | Admin    | Delete task                              |

**Status values:** `Pending`, `InProgress`, `Done`  
**Priority values:** `Low`, `Medium`, `High`

**Task object:**
```json
{
  "task_Title": "Fix login bug",
  "task_Description": "Resolve the 401 on mobile",
  "task_Status": "Pending",
  "task_Priority": "High",
  "task_DueDate": "2025-03-01T00:00:00",
  "proj_Id": 1
}
```

**Update status (PATCH body):**
```json
"InProgress"
```

---

### Task Assignments

> All endpoints require JWT. Most require `Admin` role.

| Method | Route                                          | Role     | Description                                  |
|--------|------------------------------------------------|----------|----------------------------------------------|
| GET    | `/api/taskassignment`                          | Admin    | All assignments in admin's department         |
| GET    | `/api/taskassignment/task/{taskId}`            | Admin    | Assignments for a specific task               |
| GET    | `/api/taskassignment/employee/{empId}`         | Any      | Employee: own assignments; Admin: dept employee |
| POST   | `/api/taskassignment/assign`                   | Admin    | Assign employee to a task                    |
| DELETE | `/api/taskassignment/{assignmentId}`           | Admin    | Remove assignment by ID                      |
| DELETE | `/api/taskassignment/task/{taskId}/employee/{empId}` | Admin | Remove assignment by task + employee   |

**Assignment object:**
```json
{
  "task_Id": 5,
  "emp_Id": 3
}
```

---

## Database Schema

```
Departments          Employees
──────────           ─────────
Dept_Id (PK)  ◄──── Dept_Id (FK, nullable)
Dept_Name            Emp_Id (PK)
Dept_Description     Emp_Fname
                     Emp_Lname
                     Emp_MobileNumber
                     Emp_Password
                     IsAdmin

Projects             ProjectEmployees (junction)
────────             ──────────────────────────
Proj_Id (PK)  ◄──── Proj_Id (FK) ─┐ composite PK
Proj_Name            Emp_Id (FK) ──┘
Proj_Description     JoinedDate
Proj_StartDate
Proj_EndDate

Tasks                TaskAssignments (junction)
─────                ──────────────────────────
Task_Id (PK)  ◄──── Task_Id (FK)
Task_Title           Assignment_Id (PK)
Task_Description     Emp_Id (FK)
Task_Status          AssignedDate
Task_Priority
Task_DueDate
Proj_Id (FK) ───► Projects
```

---

## Business Rules

1. **Login** uses mobile number + password (plain text — hash passwords before production).
2. **JWT token** embeds employee ID, department ID, name, mobile, and role.
3. **AdminOnly policy** restricts write operations on tasks and assignments.
4. **Department scoping**: An admin can only manage employees, tasks, and assignments within their own department (`dept_id` from JWT).
5. **Task assignment**: The assigned employee must belong to the same department as the admin performing the assignment.
6. **Project scoping**: A task belongs to a project; a project is considered "in a department" if any of its assigned employees belong to that department.
7. **Status updates**: Employees can only update the status of tasks they are personally assigned to.
8. **Cascade deletes**: Deleting a project cascades to its tasks and task assignments. Deleting an employee also cascades their task assignments.
