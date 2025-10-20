using Gruppe4NLA.DataContext;
using Microsoft.EntityFrameworkCore;
using Gruppe4NLA.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("OurDbConnection"), 
    new MySqlServerVersion(new Version(11, 8, 3))));



builder.Services
    .AddDefaultIdentity<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Optional: cookie paths so redirects go to Identity pages
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Create a test user to Login with
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();

    const string email = "test@test.com";
    const string password = "Test123!";
    const string adminRole = "Admin";

    // 1) Ensure role exists
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    // 2) Ensure user exists
    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            throw new Exception(string.Join(", ", create.Errors.Select(e => e.Description)));
    }

    // 3) Ensure user is in role
    if (!await userManager.IsInRoleAsync(user, adminRole))
    {
        await userManager.AddToRoleAsync(user, adminRole);

       
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

// Needed to load local leaflet map
app.UseStaticFiles();

app.MapRazorPages();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();




app.Run();
