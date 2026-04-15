using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Models;

namespace TaskSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        //-------------------------------- Profile --------------------------------
        //[HttpGet("profile")]
        //public IActionResult Profile()
        //{
        //    return Ok(new { Mobile = User.Identity?.Name });
        //}

        //-------------------------------- Get All --------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _context.Employees.ToListAsync();
            return Ok(employees);
        }

        //-------------------------------- Get By Id -------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound("Employee not found");
            return Ok(emp);
        }

        //-------------------------------- Add ------------------------------------
        [HttpPost]
        public async Task<IActionResult> Add(Employee emp)
        {
            try
            {
                await _context.Employees.AddAsync(emp);
                await _context.SaveChangesAsync();
                return Ok(emp);
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
        public async Task<IActionResult> Update(int id, Employee updatedEmp)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound("Employee not found");

            emp.Emp_Fname = updatedEmp.Emp_Fname;
            emp.Emp_Lname = updatedEmp.Emp_Lname;
            emp.Emp_MobileNumber = updatedEmp.Emp_MobileNumber;
            emp.Emp_Password = updatedEmp.Emp_Password;

            await _context.SaveChangesAsync();
            return Ok(emp);
        }

        //-------------------------------- Delete ---------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound("Employee not found");

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return Ok("Deleted successfully");
        }
    }
}