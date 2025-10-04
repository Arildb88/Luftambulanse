using Gruppe4NLA.DataContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string (debug output)
var connectionString = builder.Configuration.GetConnectionString("OurDbConnection");
Console.WriteLine("Using connection: " + connectionString);

// Add required services
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(); // <-- Required to fix the error

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(11, 8, 3)))
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization(); // <-- Uses the service registered above

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
