using Microsoft.EntityFrameworkCore;
using OrientalOasis.DataAccess.Data;
using OrientalOasis.DataAccess.Repository;
using OrientalOasis.DataAccess.Repository.IRepository;
using Oriental_Oasis_Web;
using Microsoft.AspNetCore.Identity;
using OrientalOasis.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Enitty framework to this project and register the class name that has the impletementation
builder.Services.AddDbContext<ApplicationDbContext>(options=> 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //specify the option is MS SQL server and passed in the name of the connection string

//register stripesetting
builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));


// Configure Identity and disable email confirmation requirement
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//deny user access to admin area
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});

//configure session for shoppingcart to show items added to it
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

//register for razor view pages which auto generated after identity configured
builder.Services.AddRazorPages();

//register service dependency injection  for the  server to find the ApplicationDbContext
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//register for the EmailSender
builder.Services.AddScoped<IEmailSender, EmailSender>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey=builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();

app.UseAuthentication();//check user sign in info is valid
//app.UseAuthorization();
app.UseAuthorization(); // Checks if the user is authorized to access the resources

//
app.UseSession();


app.MapRazorPages(); //add the Razor map in routing
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
