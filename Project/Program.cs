using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Needed for MariaDbServerVersion
using Microsoft.AspNetCore.HttpOverrides;


// Starts the web application builder
var builder = WebApplication.CreateBuilder(args);

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Only trust one hop out - the reverse proxy sitting directly in front of this
    // container. Without this, ForwardLimit defaults to 1 anyway, but we set it
    // explicitly so a client can't smuggle extra X-Forwarded-For entries past the proxy.
    options.ForwardLimit = 1;

    // The default KnownProxies/KnownNetworks only trusts loopback. On the NAS the
    // reverse proxy is a separate container/process reachable over the docker network,
    // not loopback, so those defaults would silently make the middleware ignore the
    // forwarded headers instead of erroring - the bug is invisible until you notice
    // https redirects/client IPs are wrong. Clearing them trusts whatever sent the
    // headers, which is fine ONLY because the container port is reachable exclusively
    // through the WireGuard tunnel to Oracle, not exposed directly to the internet.
    // If that changes, or the tunnel's source IP/subnet is known and stable, prefer
    // pinning KnownProxies/KnownNetworks to it instead of clearing them.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Builds the app
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseForwardedHeaders();

// Content security policy CSP
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    // OSM public tiles require a Referer (osm.wiki/Blocked). no-referrer caused 403 Access blocked.
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
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
             "https://server.arcgisonline.com " +
             "https://*.arcgisonline.com " +
             "https://*.google.com; " +
         "connect-src 'self' https://server.arcgisonline.com https://*.arcgisonline.com; " + 
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
            // Roles are looked up by their normalized (upper-case) name, so a role seeded with
            // different casing is still "found" here - but the sign-in cookie then carries the
            // stored casing, and IsInRole / [Authorize(Roles=...)] compare ordinally. Repair it.
            var existing = await roleMgr.FindByNameAsync(role);
            if (existing is null)
            {
                await roleMgr.CreateAsync(new IdentityRole(role));
            }
            else if (!string.Equals(existing.Name, role, StringComparison.Ordinal))
            {
                Console.WriteLine($"Fixing role casing: '{existing.Name}' -> '{role}'.");
                existing.Name = role;
                await roleMgr.UpdateAsync(existing);
            }
        }

        async Task EnsureUserInRole(string email, string password, string role, string? organization = null)
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
        // Makes sure the webapp doesn't stop if AspRoles doesnt exist, in regards of migrations
        Console.WriteLine($"Role/user seeding failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

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
