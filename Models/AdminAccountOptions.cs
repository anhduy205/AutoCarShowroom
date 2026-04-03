namespace AutoCarShowroom.Models
{
    public class AdminAccountOptions
    {
        public string Username { get; set; } = "admin";

        public string Password { get; set; } = "Admin@123";

        public string DisplayName { get; set; } = "Quản trị showroom";

        public string Role { get; set; } = InternalAccess.RoleAdmin;

        public bool CanAccessRevenue { get; set; } = true;

        public List<InternalAccountConfiguration> Accounts { get; set; } = [];

        public IReadOnlyList<InternalAccountConfiguration> GetAccounts()
        {
            if (Accounts.Count > 0)
            {
                return Accounts;
            }

            return
            [
                new InternalAccountConfiguration
                {
                    Username = Username,
                    Password = Password,
                    DisplayName = DisplayName,
                    Role = Role,
                    CanAccessRevenue = CanAccessRevenue
                }
            ];
        }
    }

    public class InternalAccountConfiguration
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Role { get; set; } = InternalAccess.RoleStaff;

        public bool CanAccessRevenue { get; set; }
    }
}
