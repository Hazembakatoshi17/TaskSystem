using Microsoft.AspNetCore.Mvc;
using TaskSystem.Data;
using TaskSystem.Models;
using TaskSystem.Services;

namespace TaskSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var emp = _context.Employees.FirstOrDefault(e =>
                e.Emp_MobileNumber == request.Emp_MobileNumber &&
                e.Emp_Password == request.Emp_Password);

            if (emp == null)
                return Unauthorized(new { Message = "Invalid mobile number or password" });

            var token = _jwtService.GenerateToken(emp);

            return Ok(new
            {
                Token = token,
                Name = emp.Emp_Fname + " " + emp.Emp_Lname,
                Mobile = emp.Emp_MobileNumber
            });
        }
    }
}