using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Arild: Allows access to login/register pages without being logged in
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");     // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPasswordConfirmation"); // optional

});


builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("OurDbConnection"), 
    new MariaDbServerVersion(new Version(11, 8, 3)),
    
    MySqlOptions => MySqlOptions.EnableRetryOnFailure()
    ));



builder.Services
    .AddDefaultIdentity<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Arild: Users need to login to use our application
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Optional: cookie paths so redirects go to Identity pages
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Create roles and demo users
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    //Run migrations on startup with a simple retry in case DB container is not ready yet
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var db = sp.GetRequiredService<AppDbContext>();

    const int maxAttempts = 10;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();   // applies all pending migrations
            logger.LogInformation("Database migrated successfully.");
            break;
        }
        catch (Exception ex)
        {
            if (attempt == maxAttempts) throw;
            logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed. Retrying in 2s…", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();

    // 1) Roles in our project
    string[] roles = { "Admin", "Caseworker", "CaseworkerAdm", "Pilot" };
    foreach (var role in roles)
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    // 2) Demo users to try to login to our application
    async Task EnsureUserInRole(string email, string password, string role)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var create = await userMgr.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new Exception(string.Join(", ", create.Errors.Select(e => e.Description)));
        }
        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }

    await EnsureUserInRole("admin@test.com", "Test123!", "Admin");      // Admin user
    await EnsureUserInRole("admin2@test.com", "Test123!", "Admin");      // Admin user

    await EnsureUserInRole("caseworker@test.com", "Test123!", "Caseworker"); // Caseworker user
    await EnsureUserInRole("caseworker2@test.com", "Test123!", "Caseworker"); // Caseworker user

    await EnsureUserInRole("caseworkeradm@test.com", "Test123!", "CaseworkerAdm"); // CaseworkerAdmin user
    await EnsureUserInRole("caseworkeradm2@test.com", "Test123!", "CaseworkerAdm"); // CaseworkerAdmin user

    await EnsureUserInRole("pilot@test.com", "Test123!", "Pilot");      // Pilot user
    await EnsureUserInRole("pilot2@test.com", "Test123!", "Pilot");      // Pilot user
    await EnsureUserInRole("pilot3@test.com", "Test123!", "Pilot");      // Pilot user


}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Needed to load local leaflet map
app.UseStaticFiles();
app.UseRouting();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Needed for Razor Pages-routing
app.MapRazorPages();





app.Run();
