using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VistosFacil.Infrastructure.Data;
using VistosFacil.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var conn = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(conn, ServerVersion.AutoDetect(conn),
        x => x.MigrationsAssembly("VistosFacil.Infrastructure")));

builder.Services.AddScoped<ArticleRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<SiteConfigRepository>();
builder.Services.AddScoped<NewsletterRepository>();
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => {
        o.LoginPath = "/admin/login";
        o.LogoutPath = "/admin/logout";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.Cookie.Name = "vistosfacil.admin";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/erro");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("article", "artigo/{slug}", new { controller = "Home", action = "Article" });
app.MapControllerRoute("category", "categoria/{slug}", new { controller = "Home", action = "Category" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
