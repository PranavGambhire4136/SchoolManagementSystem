using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddControllersWithViews();

var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("dbcs")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
{
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireUppercase = false;
    option.Password.RequireLowercase = false;
    option.Password.RequireDigit = false;
    option.Password.RequiredLength = 1;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    var adminEmail = "admin@gmail.com";
    var password = "admin";

    var existingAdminRole = await roleManger.FindByNameAsync("Admin");
    var existingStudentRole = await roleManger.FindByNameAsync("Student");
    var existingTeacherRole = await roleManger.FindByNameAsync("Teacher");

    if (existingStudentRole == null)
    {
        await roleManger.CreateAsync(new IdentityRole("Student"));
    }
    if (existingTeacherRole == null)
    {
        await roleManger.CreateAsync(new IdentityRole("Teacher"));
    }
    if (existingAdminRole == null)
    {
        await roleManger.CreateAsync(new IdentityRole("Admin"));
    }

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
        };
        var result = await userManager.CreateAsync(admin, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
