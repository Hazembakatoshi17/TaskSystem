using System.Security.Claims;

namespace TaskSystem.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetEmpId(this ClaimsPrincipal user)
        {
            var val = user.FindFirstValue("id");
            return int.TryParse(val, out var id) ? id : 0;
        }

        public static int? GetDeptId(this ClaimsPrincipal user)
        {
            var val = user.FindFirstValue("dept_id");
            return int.TryParse(val, out var id) ? id : null;
        }

        public static bool GetIsAdmin(this ClaimsPrincipal user)
        {
            var val = user.FindFirstValue("is_admin");
            return val == "true";
        }
    }
}
