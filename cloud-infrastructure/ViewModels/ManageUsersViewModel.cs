namespace cloud_infrastructure.ViewModels
{
    public class ManageUsersViewModel
    {
        public List<UserWithRoles> Users { get; set; } = new();
    }

    public class UserWithRoles
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsAdmin => Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
    }
}
