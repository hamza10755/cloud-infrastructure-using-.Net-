using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cloud_infrastructure.Models; // This ensures it finds your Developer and DbContext classes

var builder = WebApplication.CreateBuilder(args);

// 1. Register your Database Connection FIRST
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=cloudportal.db")); // This will create a local SQLite file for testing

// 2. Register Identity NEXT
builder.Services.AddDefaultIdentity<Developer>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 3. Turn on the Authentication Engine (MUST be exactly here, before Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();