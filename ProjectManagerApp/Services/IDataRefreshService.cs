namespace ProjectManagementSystem.WPF.Services
{
    [Flags]
    public enum DataRefreshScope
    {
        None = 0,
        Dashboard = 1,
        Projects = 2,
        MyProjects = 4,
        Tasks = 8,
        Users = 16,
        All = Dashboard | Projects | MyProjects | Tasks | Users
    }

    public static class DataRefreshScopes
    {
        public const DataRefreshScope TaskRelated =
            DataRefreshScope.Tasks | DataRefreshScope.MyProjects | DataRefreshScope.Dashboard | DataRefreshScope.Users;

        public const DataRefreshScope ProjectRelated =
            DataRefreshScope.Projects | DataRefreshScope.MyProjects | DataRefreshScope.Dashboard | DataRefreshScope.Users;

        public const DataRefreshScope UserRelated =
            DataRefreshScope.Users | DataRefreshScope.Dashboard;

        public const DataRefreshScope CommentRelated =
            DataRefreshScope.Dashboard | DataRefreshScope.Projects | DataRefreshScope.MyProjects;
    }

    public interface IDataRefreshService
    {
        event EventHandler<DataRefreshScope>? Changed;

        void Notify(DataRefreshScope scope);
    }
}
