using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Capycom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connection = builder.Configuration.GetConnectionString("OurDB");
            builder.Configuration.AddJsonFile("privateData.json");
            if (connection==String.Empty)
            {
                connection = builder.Configuration.GetSection("Test1")["OurDB"]; 
            }


            //ƒобавл€ем DB как встривание зависимости. 
            builder.Services.AddDbContext<CapycomContext>(options => options.UseSqlServer(connection));

            builder.Services.Configure<MyConfig>((options => builder.Configuration.GetSection("MyConfig").Bind(options)));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/UserLogIn") ;

            builder.Services.AddAuthorization();


			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("CpcmCanEditRoles", policy => policy.RequireClaim("CpcmCanEditRoles", "True"));
			});




			// Add services to the container.
			builder.Services.AddControllersWithViews();

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
