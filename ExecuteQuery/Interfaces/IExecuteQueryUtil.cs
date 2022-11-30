namespace ExecuteQuery.Interfaces
{
    public interface IExecuteQueryUtil
    {
        Task<IList<EntityType>> ExecuteQuery<EntityType>(string query, bool isEnvironmentVariable) where EntityType : class;
    }
}
