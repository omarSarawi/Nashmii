using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using test2.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<test2Context>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("test2Context")!,
        new MySqlServerVersion(new Version(8, 0, 34)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddAuthentication("default")
    .AddCookie("default", options =>
    {
        options.LoginPath = "/Users/Login";
        options.AccessDeniedPath = "/Users/AccessDeniedRedirect";
        options.Cookie.Name = "userCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseAuthentication();

app.UseAuthorization();
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
