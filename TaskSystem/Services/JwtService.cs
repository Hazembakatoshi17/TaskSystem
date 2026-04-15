using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskSystem.Models;

namespace TaskSystem.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(Employee emp)
        {
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id",       emp.Emp_Id.ToString()),
                new Claim("mobile",   emp.Emp_MobileNumber),
                new Claim("name",     emp.Emp_Fname + " " + emp.Emp_Lname),
                new Claim("dept_id",  emp.Dept_Id?.ToString() ?? ""),
                new Claim("is_admin", emp.IsAdmin.ToString().ToLower()),   // "true" / "false"
                // Map IsAdmin to the standard Role claim so [Authorize(Roles="Admin")] works
                new Claim(ClaimTypes.Role, emp.IsAdmin ? "Admin" : "Employee")
            };

            var token = new JwtSecurityToken(
                issuer:            _config["JwtSettings:Issuer"],
                audience:          _config["JwtSettings:Audience"],
                claims:            claims,
                expires:           DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:ExpireDays"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
