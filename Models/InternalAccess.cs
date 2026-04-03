using System.Security.Claims;

namespace AutoCarShowroom.Models
{
    public static class InternalAccess
    {
        public const string RoleAdmin = "Admin";
        public const string RoleStaff = "Staff";
        public const string BackOfficeRoles = RoleAdmin + "," + RoleStaff;
        public const string PermissionClaimType = "permission";
        public const string RevenuePermission = "revenue";
        public const string RevenuePolicy = "RevenueAccess";

        public static bool IsBackOffice(ClaimsPrincipal user)
        {
            return user.IsInRole(RoleAdmin) || user.IsInRole(RoleStaff);
        }

        public static bool CanAccessRevenue(ClaimsPrincipal user)
        {
            return user.IsInRole(RoleAdmin) || user.HasClaim(PermissionClaimType, RevenuePermission);
        }

        public static string NormalizeRole(string? role)
        {
            return string.Equals(role, RoleAdmin, StringComparison.OrdinalIgnoreCase)
                ? RoleAdmin
                : RoleStaff;
        }
    }
}
