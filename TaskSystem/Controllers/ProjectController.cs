using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Models;

namespace TaskSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        //-------------------------------- Get All --------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.Projects
                .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
                .Include(p => p.Tasks)
                .ToListAsync();
            return Ok(projects);
        }

        //-------------------------------- Get By Id ------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Proj_Id == id);

            if (project == null) return NotFound("Project not found");
            return Ok(project);
        }

        //-------------------------------- Add ------------------------------------
        [HttpPost]
        public async Task<IActionResult> Add(Project project)
        {
            try
            {
                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();
                return Ok(project);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        //-------------------------------- Update ---------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project updatedProject)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Project not found");

            project.Proj_Name        = updatedProject.Proj_Name;
            project.Proj_Description = updatedProject.Proj_Description;
            project.Proj_StartDate   = updatedProject.Proj_StartDate;
            project.Proj_EndDate     = updatedProject.Proj_EndDate;

            await _context.SaveChangesAsync();
            return Ok(project);
        }

        //-------------------------------- Delete ---------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok("Deleted successfully");
        }

        //-------------------------------- Assign Employee ------------------------
        [HttpPost("{projId}/assign/{empId}")]
        public async Task<IActionResult> AssignEmployee(int projId, int empId)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Proj_Id == projId);
            if (!projectExists) return NotFound("Project not found");

            var empExists = await _context.Employees.AnyAsync(e => e.Emp_Id == empId);
            if (!empExists) return NotFound("Employee not found");

            var alreadyAssigned = await _context.ProjectEmployees
                .AnyAsync(pe => pe.Proj_Id == projId && pe.Emp_Id == empId);
            if (alreadyAssigned) return BadRequest("Employee already assigned to this project");

            var pe = new ProjectEmployee
            {
                Proj_Id    = projId,
                Emp_Id     = empId,
                JoinedDate = DateTime.UtcNow
            };

            await _context.ProjectEmployees.AddAsync(pe);
            await _context.SaveChangesAsync();
            return Ok("Employee assigned to project successfully");
        }

        //-------------------------------- Remove Employee ------------------------
        [HttpDelete("{projId}/remove/{empId}")]
        public async Task<IActionResult> RemoveEmployee(int projId, int empId)
        {
            var pe = await _context.ProjectEmployees
                .FirstOrDefaultAsync(pe => pe.Proj_Id == projId && pe.Emp_Id == empId);

            if (pe == null) return NotFound("Assignment not found");

            _context.ProjectEmployees.Remove(pe);
            await _context.SaveChangesAsync();
            return Ok("Employee removed from project successfully");
        }
    }
}
