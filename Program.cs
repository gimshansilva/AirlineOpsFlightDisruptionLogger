using System.Data;
using AirlineOpsFlightDisruptionLogger.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AirlineOpsDb")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Disruptions/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Disruptions}/{action=Index}/{id?}");

await DatabaseInitializer.InitializeAsync(builder.Configuration.GetConnectionString("AirlineOpsDb"));

app.Run();
