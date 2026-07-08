using Frontend.Helpers;
using Frontend.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("API", client =>
{
	//client.BaseAddress = new Uri("https://localhost:7233/api/"); 
	client.BaseAddress = new Uri("http://localhost:5236/api/");
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ApiClientHelper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

    // Running frontend on HTTP for local development; do not redirect to HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
