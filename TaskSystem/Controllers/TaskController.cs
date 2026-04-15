using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Extensions;
using TaskSystem.Models;

namespace TaskSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]                         // All endpoints require a valid JWT
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: Employee sees only their own assigned tasks ──────────────────
        // ── Admin sees all tasks that belong to their department's projects ───
        [HttpGet]
        public async Task<IActionResult> GetMyTasks()
        {
            var empId   = User.GetEmpId();
            var deptId  = User.GetDeptId();
            var isAdmin = User.GetIsAdmin();

            if (isAdmin)
            {
                // Admin: return all tasks in projects that have at least one
                // employee from the same department
                var tasks = await _context.Tasks
                    .Where(t => t.Project.ProjectEmployees
                        .Any(pe => pe.Employee.Dept_Id == deptId))
                    .Include(t => t.Project)
                    .Include(t => t.TaskAssignments)
                        .ThenInclude(ta => ta.Employee)
                    .ToListAsync();

                return Ok(tasks);
            }

            // Regular employee: return only tasks assigned to them
            var myTasks = await _context.Tasks
                .Where(t => t.TaskAssignments.Any(ta => ta.Emp_Id == empId))
                .Include(t => t.Project)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.Employee)
                .ToListAsync();

            return Ok(myTasks);
        }

        // ── GET /{id}: Employee can only fetch a task they are assigned to ────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empId   = User.GetEmpId();
            var deptId  = User.GetDeptId();
            var isAdmin = User.GetIsAdmin();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.TaskAssignments)
                    .ThenInclude(ta => ta.Employee)
                .FirstOrDefaultAsync(t => t.Task_Id == id);

            if (task == null) return NotFound("Task not found");

            if (!isAdmin)
            {
                // Employee can only see tasks they are personally assigned to
                bool isAssigned = task.TaskAssignments.Any(ta => ta.Emp_Id == empId);
                if (!isAssigned) return Forbid();
            }
            else
            {
                // Admin can only see tasks within their own department's scope
                bool inDept = task.Project.ProjectEmployees?
                    .Any(pe => pe.Employee.Dept_Id == deptId) ?? false;

                // Re-query with navigation if needed
                var projWithEmps = await _context.Projects
                    .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
                    .FirstOrDefaultAsync(p => p.Proj_Id == task.Proj_Id);

                bool deptMatch = projWithEmps?.ProjectEmployees
                    .Any(pe => pe.Employee.Dept_Id == deptId) ?? false;

                if (!deptMatch) return Forbid();
            }

            return Ok(task);
        }

        // ── GET /project/{projId}: Tasks in a project (admin only) ────────────
        [HttpGet("project/{projId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetByProject(int projId)
        {
            var deptId = User.GetDeptId();

            // Confirm the project belongs to admin's department scope
            var project = await _context.Projects
                .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(p => p.Proj_Id == projId);

            if (project == null) return NotFound("Project not found");

            bool inDept = project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId);
            if (!inDept) return Forbid();

            var tasks = await _context.Tasks
                .Where(t => t.Proj_Id == projId)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.Employee)
                .ToListAsync();

            return Ok(tasks);
        }

        // ── GET /status/{status}: Filter by status (admin only) ───────────────
        [HttpGet("status/{status}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetByStatus(WorkTaskStatus status)
        {
            var deptId = User.GetDeptId();

            var tasks = await _context.Tasks
                .Where(t => t.Task_Status == status &&
                            t.Project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId))
                .Include(t => t.Project)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.Employee)
                .ToListAsync();

            return Ok(tasks);
        }

        // ── GET /priority/{priority}: Filter by priority (admin only) ─────────
        [HttpGet("priority/{priority}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetByPriority(WorkTaskPriority priority)
        {
            var deptId = User.GetDeptId();

            var tasks = await _context.Tasks
                .Where(t => t.Task_Priority == priority &&
                            t.Project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId))
                .Include(t => t.Project)
                .Include(t => t.TaskAssignments).ThenInclude(ta => ta.Employee)
                .ToListAsync();

            return Ok(tasks);
        }

        // ── POST: Create task — Admin only ────────────────────────────────────
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Add(TaskItem task)
        {
            var deptId = User.GetDeptId();

            // Verify the target project is within the admin's department
            var project = await _context.Projects
                .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(p => p.Proj_Id == task.Proj_Id);

            if (project == null) return NotFound("Project not found");

            bool inDept = project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId);
            if (!inDept)
                return Forbid();   // Admin cannot create tasks outside their department

            try
            {
                await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();
                return Ok(task);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { Error = ex.Message, InnerError = ex.InnerException?.Message });
            }
        }

        // ── PUT /{id}: Update task — Admin only ───────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, TaskItem updatedTask)
        {
            var deptId = User.GetDeptId();

            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectEmployees)
                        .ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(t => t.Task_Id == id);

            if (task == null) return NotFound("Task not found");

            bool inDept = task.Project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId);
            if (!inDept) return Forbid();

            task.Task_Title       = updatedTask.Task_Title;
            task.Task_Description = updatedTask.Task_Description;
            task.Task_Status      = updatedTask.Task_Status;
            task.Task_Priority    = updatedTask.Task_Priority;
            task.Task_DueDate     = updatedTask.Task_DueDate;
            task.Proj_Id          = updatedTask.Proj_Id;

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // ── PATCH /{id}/status: Employee updates status of their own task ─────
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] WorkTaskStatus status)
        {
            var empId   = User.GetEmpId();
            var isAdmin = User.GetIsAdmin();

            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .FirstOrDefaultAsync(t => t.Task_Id == id);

            if (task == null) return NotFound("Task not found");

            // Employee can only update status of tasks assigned to them
            if (!isAdmin)
            {
                bool isAssigned = task.TaskAssignments.Any(ta => ta.Emp_Id == empId);
                if (!isAssigned) return Forbid();
            }

            task.Task_Status = status;
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // ── DELETE /{id}: Admin only ──────────────────────────────────────────
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var deptId = User.GetDeptId();

            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectEmployees)
                        .ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(t => t.Task_Id == id);

            if (task == null) return NotFound("Task not found");

            bool inDept = task.Project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId);
            if (!inDept) return Forbid();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok("Deleted successfully");
        }
    }
}
