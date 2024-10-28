using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestTool;
using TestTool.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSqlConection(builder.Configuration["sql:connectionString"]!);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
QueryLoggerInterceptor.ConfigureLogger(logger);

await host.RunAsync();

var sqlConnection = host.Services.GetRequiredService<SqlConnection>();
var sqlQuery = "SELECT * FROM Apps";

await sqlConnection.QueryAsync(sqlQuery);