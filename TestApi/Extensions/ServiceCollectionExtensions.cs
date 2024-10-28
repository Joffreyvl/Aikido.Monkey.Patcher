using Microsoft.Data.SqlClient;

namespace TestApi.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddSqlConection(this IServiceCollection services, string connectionString)
        => services.AddSingleton(new SqlConnection(connectionString));
}
