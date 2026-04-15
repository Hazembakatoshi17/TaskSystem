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
    [Authorize]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskAssignmentController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: Admin sees all assignments in their dept ─────────────────────
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            var deptId = User.GetDeptId();

            var assignments = await _context.TaskAssignments
                .Where(ta => ta.Employee.Dept_Id == deptId)
                .Include(ta => ta.Task).ThenInclude(t => t.Project)
                .Include(ta => ta.Employee)
                .ToListAsync();

            return Ok(assignments);
        }

        // ── GET /task/{taskId}: Assignments for a task (admin only) ───────────
        [HttpGet("task/{taskId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetByTask(int taskId)
        {
            var deptId = User.GetDeptId();

            // Verify task belongs to admin's department scope
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectEmployees)
                        .ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(t => t.Task_Id == taskId);

            if (task == null) return NotFound("Task not found");

            bool inDept = task.Project.ProjectEmployees.Any(pe => pe.Employee.Dept_Id == deptId);
            if (!inDept) return Forbid();

            var assignments = await _context.TaskAssignments
                .Where(ta => ta.Task_Id == taskId)
                .Include(ta => ta.Employee)
                .ToListAsync();

            return Ok(assignments);
        }

        // ── GET /employee/{empId}: Employee sees their own; admin sees any ─────
        [HttpGet("employee/{empId}")]
        public async Task<IActionResult> GetByEmployee(int empId)
        {
            var callerId = User.GetEmpId();
            var deptId   = User.GetDeptId();
            var isAdmin  = User.GetIsAdmin();

            // Non-admin can only query themselves
            if (!isAdmin && callerId != empId)
                return Forbid();

            // Admin can only query employees within their own department
            if (isAdmin)
            {
                var targetEmp = await _context.Employees.FindAsync(empId);
                if (targetEmp == null) return NotFound("Employee not found");
                if (targetEmp.Dept_Id != deptId) return Forbid();
            }

            var assignments = await _context.TaskAssignments
                .Where(ta => ta.Emp_Id == empId)
                .Include(ta => ta.Task).ThenInclude(t => t.Project)
                .ToListAsync();

            return Ok(assignments);
        }

        // ── POST /assign: Assign employee to task — Admin only ────────────────
        // Business rule: Employee must be in the same department as the admin
        [HttpPost("assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Assign([FromBody] TaskAssignment assignment)
        {
            var adminDeptId = User.GetDeptId();

            // 1. Task must exist
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectEmployees)
                        .ThenInclude(pe => pe.Employee)
                .FirstOrDefaultAsync(t => t.Task_Id == assignment.Task_Id);

            if (task == null) return NotFound("Task not found");

            // 2. Task must be within admin's department scope
            bool taskInDept = task.Project.ProjectEmployees
                .Any(pe => pe.Employee.Dept_Id == adminDeptId);

            if (!taskInDept)
                return BadRequest(new { Error = "You can only manage tasks within your own department." });

            // 3. Employee must exist
            var employee = await _context.Employees.FindAsync(assignment.Emp_Id);
            if (employee == null) return NotFound("Employee not found");

            // 4. Employee must be in the same department as the admin
            if (employee.Dept_Id != adminDeptId)
                return BadRequest(new
                {
                    Error = $"Employee does not belong to your department. " +
                            $"Only employees from department {adminDeptId} can be assigned."
                });

            // 5. No duplicate assignment
            bool alreadyAssigned = await _context.TaskAssignments
                .AnyAsync(ta => ta.Task_Id == assignment.Task_Id && ta.Emp_Id == assignment.Emp_Id);

            if (alreadyAssigned)
                return BadRequest(new { Error = "Employee is already assigned to this task." });

            assignment.AssignedDate = DateTime.UtcNow;

            try
            {
                await _context.TaskAssignments.AddAsync(assignment);
                await _context.SaveChangesAsync();
                return Ok(assignment);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { Error = ex.Message, InnerError = ex.InnerException?.Message });
            }
        }

        // ── DELETE /{assignmentId}: Remove assignment — Admin only ─────────────
        [HttpDelete("{assignmentId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Unassign(int assignmentId)
        {
            var deptId = User.GetDeptId();

            var assignment = await _context.TaskAssignments
                .Include(ta => ta.Employee)
                .FirstOrDefaultAsync(ta => ta.Assignment_Id == assignmentId);

            if (assignment == null) return NotFound("Assignment not found");

            // Admin can only remove assignments within their department
            if (assignment.Employee.Dept_Id != deptId) return Forbid();

            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok("Assignment removed successfully");
        }

        // ── DELETE /task/{taskId}/employee/{empId} ────────────────────────────
        [HttpDelete("task/{taskId}/employee/{empId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UnassignByTaskAndEmployee(int taskId, int empId)
        {
            var deptId = User.GetDeptId();

            var employee = await _context.Employees.FindAsync(empId);
            if (employee == null) return NotFound("Employee not found");

            if (employee.Dept_Id != deptId) return Forbid();

            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(ta => ta.Task_Id == taskId && ta.Emp_Id == empId);

            if (assignment == null) return NotFound("Assignment not found");

            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok("Assignment removed successfully");
        }
    }
}
