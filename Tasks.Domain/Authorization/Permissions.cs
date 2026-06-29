namespace Tasks.Domain.Authorization
{
    public static class Permissions
    {
        public const string ClaimType = "permission";

        public static class Corporations
        {
            public const string Manage = "Corporations.Manage";
        }

        public static class Sections
        {
            public const string Manage = "Sections.Manage";
        }

        public static class TaskTypes
        {
            public const string Manage = "TaskTypes.Manage";
        }

        public static class Users
        {
            public const string Manage = "Users.Manage";
        }

        public static class Tasks
        {
            public const string Create          = "Tasks.Create";
            public const string ViewAll         = "Tasks.ViewAll";
            public const string ViewAssigned    = "Tasks.ViewAssigned";
            public const string Comment         = "Tasks.Comment";
            public const string UpdateProgress  = "Tasks.UpdateProgress";
            public const string Review          = "Tasks.Review";
        }

        public static IEnumerable<string> GetAll()
        {
            yield return Corporations.Manage;
            yield return Sections.Manage;
            yield return TaskTypes.Manage;
            yield return Users.Manage;
            yield return Tasks.Create;
            yield return Tasks.ViewAll;
            yield return Tasks.ViewAssigned;
            yield return Tasks.Comment;
            yield return Tasks.UpdateProgress;
            yield return Tasks.Review;
        }
    }
}
