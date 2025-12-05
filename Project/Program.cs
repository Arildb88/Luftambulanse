using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Needed for MariaDbServerVersion

// Starts the web application builder
var builder = WebApplication.CreateBuilder(args);

var stadiaApiKey = builder.Configuration["ApiKeys:StadiaMaps"];

// Add services to the container (adds Antiforgery token validation globally to all unsafe HTTP methods for MVC controllers Post/Put/Patch/Delete)
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");     // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword"); // optional
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/ForgotPasswordConfirmation"); // optional
});

// Hide "Server" header from Kestrel, security measures. Also added to the group of security measures below
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

builder.Services.AddScoped<IReportAssignmentService, ReportAssignmentService>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    options.AddPolicy("CanAssignReports", p => p.RequireRole("CaseworkerAdm"));
});

// Optional: cookie paths so redirects go to Identity pages
// Users trying to access a restricted page will be redirected to the login page
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Builds the app
var app = builder.Build();

app.Use(async (context, next) =>
{
    var config = context.RequestServices.GetRequiredService<IConfiguration>();
    context.Items["StadiaApiKey"] = config["ApiKeys:StadiaMaps"];
    await next();
});


// Content security policy CSP
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers.Remove("Server"); //Already removed with the addServerHeader=false, but now within the security measures. Unacecerry but not harmful

    if (context.Request.IsHttps)
        context.Response.Headers["Strict-Transport-Security"] =
            "max-age=31536000; includeSubDomains; preload";

    // Needs to be "whitelisted" to allow Leaflet to function properly
    // Allow Leaflet + the tile hosts you actually use
    context.Response.Headers["Content-Security-Policy"] =
         "default-src 'self'; " +
         "script-src 'self' 'unsafe-inline' https://unpkg.com https://cdn.jsdelivr.net; " +
         "style-src  'self' 'unsafe-inline' https://unpkg.com https://cdn.jsdelivr.net; " +
         "img-src 'self' data: blob: " +
             "https://tile.openstreetmap.org " +
             "https://*.tile.openstreetmap.org " +
             "https://tiles.stadiamaps.com " +
             "https://*.stadiamaps.com " + 
             "https://*.google.com; " +
         "connect-src 'self' https://tiles.stadiamaps.com https://*.stadiamaps.com; " + 
         "font-src 'self' data:; " +
         "frame-src 'self'; " +
         "frame-ancestors 'self'; " +
         "base-uri 'self'; form-action 'self'";

    await next();
});

// Create roles and demo users
using (var scope = app.Services.CreateScope())
{
    try
    {
        var sp = scope.ServiceProvider;

        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Caseworker", "CaseworkerAdm", "Pilot" };
        foreach (var role in roles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                await roleMgr.CreateAsync(new IdentityRole(role));
            }
        }

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
            else
            {
                if (!string.IsNullOrWhiteSpace(organization) && user.Organization != organization)
                {
                    user.Organization = organization;
                    await userMgr.UpdateAsync(user);
                }
            }

            if (!await userMgr.IsInRoleAsync(user, role))
                await userMgr.AddToRoleAsync(user, role);
        }

        await EnsureUserInRole("admin@test.com", "Test123!", "Admin");
        await EnsureUserInRole("admin1@test.com", "Test123!", "Admin");

        await EnsureUserInRole("caseworker@test.com", "Test123!", "Caseworker");
        await EnsureUserInRole("caseworker1@test.com", "Test123!", "Caseworker");
        await EnsureUserInRole("caseworker2@test.com", "Test123!", "Caseworker");

        await EnsureUserInRole("caseworkeradm@test.com", "Test123!", "CaseworkerAdm");
        await EnsureUserInRole("caseworkeradm1@test.com", "Test123!", "CaseworkerAdm");
        await EnsureUserInRole("caseworkeradm2@test.com", "Test123!", "CaseworkerAdm");

        await EnsureUserInRole("pilot@test.com", "Test123!", "Pilot", "AvdNord");
        await EnsureUserInRole("pilot1@test.com", "Test123!", "Pilot", "AvdSørØst");
        await EnsureUserInRole("pilot2@test.com", "Test123!", "Pilot", "AvdSørVest");
        await EnsureUserInRole("pilot3@test.com", "Test123!", "Pilot", "AvdSør");
    }
    catch (Exception ex)
    {
        // Hindrer at hele appen dør hvis seeding feiler
        Console.WriteLine($"Role/user seeding failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

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

app.MapRazorPages();

app.Run();
