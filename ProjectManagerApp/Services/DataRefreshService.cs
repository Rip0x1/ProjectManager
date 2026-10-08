namespace ProjectManagementSystem.WPF.Services
{
    public class DataRefreshService : IDataRefreshService
    {
        public event EventHandler<DataRefreshScope>? Changed;

        public void Notify(DataRefreshScope scope)
        {
            if (scope == DataRefreshScope.None)
            {
                return;
            }

            Changed?.Invoke(this, scope);
        }
    }
}
