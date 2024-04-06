using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Serilog.Formatting.Json;
using Serilog.Core;

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


            //��������� DB ��� ���������� �����������. 
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
			Log.Logger = new LoggerConfiguration()
				.Destructure.With<SerializationPolicy>()
                .MinimumLevel.Debug() //Information()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Error) //��� ������� �� Microsoft, Microsoft.AspNetCore, Microsoft.AspNetCore.Hosting � �.�., ����� ������������ �� ������ Information � ����.
				.Enrich.FromLogContext()
	            .WriteTo.Console()
	            .WriteTo.Async(a=> a.File(path:"Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(30))) //formatter: new JsonFormatter()
				.WriteTo.Async(a=> a.MSSqlServer(
		            connectionString: builder.Configuration.GetSection("Test1")["OurDB"],
					columnOptions: columnOptions,
					sinkOptions: new MSSqlServerSinkOptions { TableName = "CPCM_LogEvents", AutoCreateSqlTable = true, }))
	            .CreateLogger();

			builder.Host.UseSerilog();
			Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));


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
	class SerializationPolicy : IDestructuringPolicy
	{
		public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
		{
			var type = value.GetType();
			if (type.Namespace.Contains("Capycom"))
			{
				// ������� ����� ��������� ���������� ���� � ���� �� ����������, ��� � �������� ������,
				// �� ��������� ��������, ������� ����� ����������� ������������ ���� Capycom
				var properties = type.GetProperties()
					.Where(p => !IsFromCapycomNamespace(p.PropertyType))
					.ToDictionary(p => p.Name, p => p.GetValue(value));

				result = propertyValueFactory.CreatePropertyValue(properties, false);
				return true;
			}

			// ��� ���� ��������� ����� �������� ������������ ����������� ������������
			result = propertyValueFactory.CreatePropertyValue(value, true);
			return true;
		}
		private bool IsFromCapycomNamespace(Type type)
		{
			while (type != null)
			{
				if (type.Namespace != null && type.Namespace.Contains("Capycom"))
				{
					return true;
				}

				if (type.IsGenericType)
				{
					foreach (var argument in type.GetGenericArguments())
					{
						if (IsFromCapycomNamespace(argument))
						{
							return true;
						}
					}
				}

				type = type.BaseType;
			}

			return false;
		}
	}
}
