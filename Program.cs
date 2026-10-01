using HCMPortal.Helper;
using HCMPortal.Interface;
using HCMPortal.Middleware;
using HCMPortal.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
//Common.InitConfigure(builder.Configuration);
// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
// This registers the service and automatically injects HttpClient and Configuration
builder.Services.AddHttpClient<ZohoAuthService>();
builder.Services.AddScoped<SaveAmsNotification>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/CustomerManagement/Login";
        options.LogoutPath = "/CustomerManagement/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

Common.InitConfigure(builder.Configuration, httpContextAccessor, app.Services);

//// new added HTML pages
//// --- SETUP FOR YOUR CUSTOM HTML FOLDER ---
//var htmlPath = Path.Combine(builder.Environment.ContentRootPath, "HTMLPages");

//// 1. Set the Default File (Landing Page) logic for the custom folder
//DefaultFilesOptions defOptions = new DefaultFilesOptions();
//defOptions.FileProvider = new PhysicalFileProvider(htmlPath);
//defOptions.DefaultFileNames.Clear();
//defOptions.DefaultFileNames.Add("index.html");
//app.UseDefaultFiles(defOptions);

//// 2. Allow the app to serve files from the "HTMLPages" folder
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(htmlPath),
//    RequestPath = "" // This makes it appear at the root "/"
//});







// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    // Catches unhandled server/code exceptions and re-executes the request to /Home/Error
    app.UseExceptionHandler("/Home/Error");

    // Catches HTTP status code errors (e.g., 404, 403) and passes the status code to /Home/Error/{0}
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();

// Use merged middleware
app.UseMiddleware<HCMPortal.Middleware.SessionTimeoutMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();


//app.MapControllerRoute(
//    name: "dynamicPage",
//    pattern: "{slug}",
//    //defaults: new { controller = "DynamicPage", action = "Page" }); // Replace 'Pages' with your actual controller name
//    defaults: new { controller = "Home", action = "Blogdetails" }); // Replace 'Pages' with your actual controller name

// New route for category/slug structure




//app.MapControllerRoute(
//    name: "customersblogs",
//    pattern: "customers/blogs/{slug}",
//    defaults: new { controller = "Home", action = "Blogdetails" });
//app.MapControllerRoute(
//    name: "hcm-solution",
//    pattern: "hcm-solution/{slug}",
//    defaults: new { controller = "Home", action = "PageDetails" });
//app.MapControllerRoute(
//    name: "customers",
//    pattern: "customers/{slug}",
//    defaults: new { controller = "Home", action = "PageDetails" });
//app.MapControllerRoute(
//    name: "company",
//    pattern: "company/{slug}",
//    defaults: new { controller = "Home", action = "PageDetails" });
app.MapControllers();
// 1. Dynamic Route: This covers /Home/Blogs/{slug}, /News/Articles/{slug}, etc.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// 3. Catch-all for single-word slugs
//app.MapControllerRoute(
//    name: "dynamicPage",
//    pattern: "{slug}",
//    defaults: new { controller = "Home", action = "Blogdetails" });

//app.MapControllerRoute(
//    name: "dynamicPage",
//    pattern: "Home/{slug}", // Now it only catches /Pages/payroll-management
//    defaults: new { controller = "Home", action = "Pagedetails" });

app.Run();
