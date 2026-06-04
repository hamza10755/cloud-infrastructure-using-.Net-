using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cloud_infrastructure.Models; // This ensures it finds your Developer and DbContext classes
using cloud_infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// 1. Register your Database Connection FIRST
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=cloudportal.db")); // This will create a local SQLite file for testing

// 2. Register Identity NEXT
builder.Services.AddDefaultIdentity<Developer>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    EnsureServerInstanceStatusColumn(dbContext);
    EnsureServerInstancePurposeColumn(dbContext);
}

await IdentitySeeder.SeedAsync(app.Services);

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

app.MapRazorPages();

app.Run();

static void EnsureServerInstanceStatusColumn(ApplicationDbContext dbContext)
{
    var connection = dbContext.Database.GetDbConnection();
    var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

    if (shouldCloseConnection)
    {
        connection.Open();
    }

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('tbl_ProvisionedServers');";

        var statusColumnExists = false;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), "Status", StringComparison.OrdinalIgnoreCase))
            {
                statusColumnExists = true;
                break;
            }
        }

        if (!statusColumnExists)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE tbl_ProvisionedServers ADD COLUMN Status TEXT NOT NULL DEFAULT 'Pending';";
            alterCommand.ExecuteNonQuery();
        }
    }
    finally
    {
        if (shouldCloseConnection)
        {
            connection.Close();
        }
    }
}

static void EnsureServerInstancePurposeColumn(ApplicationDbContext dbContext)
{
    var connection = dbContext.Database.GetDbConnection();
    var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

    if (shouldCloseConnection)
    {
        connection.Open();
    }

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('tbl_ProvisionedServers');";

        var purposeColumnExists = false;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), "Purpose", StringComparison.OrdinalIgnoreCase))
            {
                purposeColumnExists = true;
                break;
            }
        }

        if (!purposeColumnExists)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE tbl_ProvisionedServers ADD COLUMN Purpose TEXT NULL;";
            alterCommand.ExecuteNonQuery();
        }
    }
    finally
    {
        if (shouldCloseConnection)
        {
            connection.Close();
        }
    }
}