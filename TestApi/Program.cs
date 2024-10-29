using Dapper;
using Microsoft.Data.SqlClient;
using TestApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlConection(builder.Configuration["sql:connectionString"]!);
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
TestApi.QueryLoggerInterceptor.ConfigureLogger(logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/", async (SqlConnection sqlConnection) =>
{
    var sqlQuery = "SELECT * FROM Apps";

    await sqlConnection.QueryAsync(sqlQuery);
})
.WithOpenApi();

await app.RunAsync();