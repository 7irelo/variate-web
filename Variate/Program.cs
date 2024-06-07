using Variate.Endpoints;
using Variate.Data;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("Variate");
builder.Services.AddSqlite<VariateContext>(connString);

var app = builder.Build();

app.MapProductsEndpoints();

app.MigrateDb();

app.Run();