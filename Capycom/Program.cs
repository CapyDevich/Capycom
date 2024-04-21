using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Configuration;
using AspNetCoreRateLimit;

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


            builder.Services.AddDbContext<CapycomContext>(options => options.UseSqlServer(connection));

            builder.Services.Configure<MyConfig>((options => builder.Configuration.GetSection("MyConfig").Bind(options)));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/UserLogIn") ;

            builder.Services.AddAuthorization();

			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("CpcmCanEditRoles", policy => policy.RequireClaim("CpcmCanEditRoles", "True"));
			});

			builder.Logging.ClearProviders();
			var columnOptions = new ColumnOptions();
			columnOptions.Store.Add(StandardColumn.LogEvent);
#if DEBUG
			Log.Logger = new LoggerConfiguration()
					.Destructure.With<SerializationPolicy>()
					.MinimumLevel.Debug() //Information()
					.MinimumLevel.Override("Microsoft", LogEventLevel.Error) //все событи€ от Microsoft, Microsoft.AspNetCore, Microsoft.AspNetCore.Hosting и т.д., будут записыватьс€ на уровне Information и выше.
					.Enrich.FromLogContext()
					.WriteTo.Console()
					.WriteTo.Async(a => a.File(path: "Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(30))) //formatter: new JsonFormatter()
					.WriteTo.Async(a => a.MSSqlServer(
						connectionString: builder.Configuration.GetSection("Test1")["OurDB"],
						columnOptions: columnOptions,
						sinkOptions: new MSSqlServerSinkOptions { TableName = "CPCM_LogEvents", AutoCreateSqlTable = true, }))
					.CreateLogger();
#elif RELEASE
			Log.Logger = new LoggerConfiguration()
					.Destructure.With<SerializationPolicy>()
					.MinimumLevel.Warning() //Information()
					.MinimumLevel.Override("Microsoft", LogEventLevel.Error) //все событи€ от Microsoft, Microsoft.AspNetCore, Microsoft.AspNetCore.Hosting и т.д., будут записыватьс€ на уровне Information и выше.
					.Enrich.FromLogContext()
					.WriteTo.Console()
					.WriteTo.Async(a => a.File(path: "Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(30))) //formatter: new JsonFormatter()
					.WriteTo.Async(a => a.MSSqlServer(
						connectionString: builder.Configuration.GetSection("Test1")["OurDB"],
						columnOptions: columnOptions,
						sinkOptions: new MSSqlServerSinkOptions { TableName = "CPCM_LogEvents", AutoCreateSqlTable = true, }))
					.CreateLogger();
#endif
			builder.Host.UseSerilog();
			Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

			builder.Services.AddControllersWithViews();

			builder.Services.AddMvc(options =>
			{
				options.Filters.Add(typeof(AuthFilter)); // ƒобавл€ем фильтр аутентификации
			});


			builder.Services.AddRateLimiter(_ => _
			.AddFixedWindowLimiter(policyName: "fixed", options =>
			{
				options.PermitLimit = 1000;
				options.Window = TimeSpan.FromSeconds(10);
				options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				options.QueueLimit = 100;
			}));

			builder.Services.AddOptions();
			builder.Services.AddMemoryCache();
			builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
			builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

			// ќстальной код...
			var app = builder.Build();
			app.UseSerilogRequestLogging();
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

			app.UseRateLimiter();

			//app.UseMiddleware<UserAuthMiddleware>();

			app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
