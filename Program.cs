using Microsoft.EntityFrameworkCore;
using OrientalOasis.DataAccess.Data;
using OrientalOasis.DataAccess.Repository;
using OrientalOasis.DataAccess.Repository.IRepository;
using Oriental_Oasis_Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Enitty framework to this project and register the class name that has the impletementation
builder.Services.AddDbContext<ApplicationDbContext>(options=> options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //specify the option is MS SQL server and passed in the name of the connection string

//register service dependency injection  for the  server to find the ApplicationDbContext
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
