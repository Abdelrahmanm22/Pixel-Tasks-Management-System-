namespace Tasks.Domain.Authorization
{
    public static class RolePermissions
    {
        private static readonly IReadOnlyDictionary<string, string[]> _map =
            new Dictionary<string, string[]>
            {
                [Roles.Admin] = new[]
                {
                    Permissions.Corporations.Manage,
                    Permissions.Sections.Manage,
                    Permissions.TaskTypes.Manage,
                    Permissions.Users.Manage,
                    Permissions.Tasks.Create,
                    Permissions.Tasks.ViewAll,
                    Permissions.Tasks.Comment,
                    Permissions.Tasks.Review,
                },
                [Roles.Employee] = new[]
                {
                    Permissions.Tasks.ViewAssigned,
                    Permissions.Tasks.Comment,
                    Permissions.Tasks.UpdateProgress,
                },
            };

        public static IEnumerable<string> GetPermissions(string role)
            => _map.TryGetValue(role, out var perms) ? perms : Enumerable.Empty<string>();

        public static IEnumerable<string> GetPermissions(IEnumerable<string> roles)
            => roles.SelectMany(GetPermissions).Distinct();
    }
}
