using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Security.Cryptography;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container (adds Antiforgery token validation globally to all unsafe HTTP methods)
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Arild: Allows access to login/register pages without being logged in
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");     // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPasswordConfirmation"); // optional

});


// Hide "Server" header from Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("OurDbConnection"), 
    new MariaDbServerVersion(new Version(11, 8, 3)),
    
    MySqlOptions => MySqlOptions.EnableRetryOnFailure()
    ));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Delegation business logic (used by CaseworkerAdmin to assign/unassign reports)
builder.Services.AddScoped<IReportAssignmentService, ReportAssignmentService>();

// Arild: Users need to login to use our application
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    // Delegation policy: only users with the CaseworkerAdm role can assign/unassign reports
    options.AddPolicy("CanAssignReports", p => p.RequireRole("CaseworkerAdm"));
});

// Optional: cookie paths so redirects go to Identity pages
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Content security policy CSP
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers.Remove("Server"); //Already removed with the addServerHeader=false, but now within the security measures. Unacecerry but not harmful

    if (context.Request.IsHttps)
        context.Response.Headers["Strict-Transport-Security"] =
            "max-age=31536000; includeSubDomains; preload";

    // Allow Leaflet + the tile hosts you actually use
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://unpkg.com https://cdn.jsdelivr.net; " +
        "style-src  'self' 'unsafe-inline' https://unpkg.com https://cdn.jsdelivr.net; " +
        // <-- IMPORTANT: tile/image sources
        "img-src 'self' data: blob: " +
            "https://tile.openstreetmap.org " +        // no-subdomain OSM
            "https://*.tile.openstreetmap.org " +      // subdomain OSM (a/b/c)
            "https://tiles.stadiamaps.com " +          // your dark tiles
            "https://*.google.com; " +                 // mt0..mt3.google.com/vt
        "connect-src 'self'; " +
        "font-src 'self' data:; " +
        "frame-src 'self'; " +                     // allows <iframe src="..."> within same origin
        "frame-ancestors 'self'; " +               // prevents embedding by *other* sites
        "base-uri 'self'; form-action 'self'";

    // Add other headers as needed
    await next();
});

// Create roles and demo users
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();

    // 1) Roles in our project
    string[] roles = { "Admin", "Caseworker", "CaseworkerAdm", "Pilot" };
    foreach (var role in roles)
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    // 2) Demo users to try to login to our application
    async Task EnsureUserInRole(string email, string password, string role, string organization = null)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Organization = organization
            };
            var create = await userMgr.CreateAsync(user, password);
            if (!create.Succeeded)
                throw new Exception(string.Join(", ", create.Errors.Select(e => e.Description)));
        }
        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }

    await EnsureUserInRole("admin@test.com", "Test123!", "Admin");      // Admin user
    await EnsureUserInRole("admin1@test.com", "Test123!", "Admin");      // Admin user
    await EnsureUserInRole("admin2@test.com", "Test123!", "Admin");      // Admin user

    await EnsureUserInRole("caseworker@test.com", "Test123!", "Caseworker"); // Caseworker user
    await EnsureUserInRole("caseworker1@test.com", "Test123!", "Caseworker"); // Caseworker user
    await EnsureUserInRole("caseworker2@test.com", "Test123!", "Caseworker"); // Caseworker user

    await EnsureUserInRole("caseworkeradm@test.com", "Test123!", "CaseworkerAdm"); // CaseworkerAdmin user
    await EnsureUserInRole("caseworkeradm1@test.com", "Test123!", "CaseworkerAdm"); // CaseworkerAdmin user
    await EnsureUserInRole("caseworkeradm2@test.com", "Test123!", "CaseworkerAdm"); // CaseworkerAdmin user

    await EnsureUserInRole("pilot@test.com", "Test123!", "Pilot", "Avd Nord");      // Pilot user
    await EnsureUserInRole("pilot1@test.com", "Test123!", "Pilot", "Avd SørØst");      // Pilot user
    await EnsureUserInRole("pilot2@test.com", "Test123!", "Pilot", "Avd SørVest");      // Pilot user
    await EnsureUserInRole("pilot3@test.com", "Test123!", "Pilot", "Avd Sør");      // Pilot user


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
