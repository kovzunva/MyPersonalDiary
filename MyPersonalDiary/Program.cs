using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPersonalDiary.Data;
using MyPersonalDiary.Interfaces;
using MyPersonalDiary.Middleware;
using MyPersonalDiary.Models;
using MyPersonalDiary.Services;
using MyPersonalDiary.Validators;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<User>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Маршрути входу та реєстрації
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

/* Services */
builder.Services.Configure<MyPersonalDiary.AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IRegistrationCodesService, RegistrationCodesService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IAccountService, AccountService>();

/* Validator Services */
builder.Services.AddTransient<IValidator<Post>, PostValidator>();

/* Background Services */
builder.Services.AddHostedService<AccountDeletionBackgroundService>();

// Session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MyPersonalDiary.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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
app.UseSession();

app.MapControllerRoute(
    name: "register",
    pattern: "Account/Register/{registrationCode?}",
    defaults: new { controller = "Account", action = "Register" });
app.MapControllerRoute(
    name: "register",
    pattern: "Register/{registrationCode?}",
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


// Middleware
app.UseMiddleware<NotFoundMiddleware>();
app.UseMiddleware<AccountDeletionMiddleware>();


app.Run();