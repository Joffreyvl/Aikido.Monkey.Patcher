using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace TestTool.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddSqlConection(this IServiceCollection services, string connectionString)
        => services.AddSingleton(new SqlConnection(connectionString));
}
