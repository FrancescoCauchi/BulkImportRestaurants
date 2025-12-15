using BulkImportRestaurants.Data;
using BulkImportRestaurants.Filters;
using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Database (EF Core + SQL Server)
// ------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ------------------------------------------------------------
// Identity (Login + Register)
// ------------------------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ------------------------------------------------------------
// MVC + Razor Pages
// ------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ------------------------------------------------------------
// Repositories
// ------------------------------------------------------------

// Holds parsed items temporarily (preview stage)
builder.Services.AddSingleton<ItemsInMemoryRepository>();

// Database repository
builder.Services.AddScoped<ItemsDbRepository>();

// Optional: interface binding (used in spec)
builder.Services.AddScoped<IItemsRepository, ItemsDbRepository>();

// ------------------------------------------------------------
// Filters
// ------------------------------------------------------------
builder.Services.AddScoped<ApprovalFilter>();

// ------------------------------------------------------------
// Build app
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// HTTP request pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------------
// Routing
// ------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ------------------------------------------------------------
// Run
// ------------------------------------------------------------
app.Run();
