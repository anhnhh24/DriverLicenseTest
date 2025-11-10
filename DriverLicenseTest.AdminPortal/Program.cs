using Microsoft.EntityFrameworkCore;
using DriverLicenseTest.Infrastructure.Data;
using DriverLicenseTest.Application.Interfaces;
using DriverLicenseTest.Application.Implementations;
using DriverLicenseTest.Application.Services.Interfaces;
using DriverLicenseTest.Application.Services;
using DriverLicenseTest.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages(options =>
{
// Set Admin/Index as default page
    options.Conventions.AddPageRoute("/Admin/Users/Index", "");
});

// Database Context
builder.Services.AddDbContext<DriverLicenseTestContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("DriverLicenseTest.Infrastructure")
    ));

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITrafficSignService, TrafficSignService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.MapRazorPages();

app.Run();
