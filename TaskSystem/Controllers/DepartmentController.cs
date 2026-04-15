using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Models;

namespace TaskSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        //-------------------------------- Get All --------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();
            return Ok(departments);
        }

        //-------------------------------- Get By Id ------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Dept_Id == id);

            if (dept == null) return NotFound("Department not found");
            return Ok(dept);
        }

        //-------------------------------- Add ------------------------------------
        [HttpPost]
        public async Task<IActionResult> Add(Department dept)
        {
            try
            {
                await _context.Departments.AddAsync(dept);
                await _context.SaveChangesAsync();
                return Ok(dept);
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
        public async Task<IActionResult> Update(int id, Department updatedDept)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound("Department not found");

            dept.Dept_Name        = updatedDept.Dept_Name;
            dept.Dept_Description = updatedDept.Dept_Description;

            await _context.SaveChangesAsync();
            return Ok(dept);
        }

        //-------------------------------- Delete ---------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound("Department not found");

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok("Deleted successfully");
        }
    }
}
