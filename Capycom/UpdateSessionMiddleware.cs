﻿#define ASYNC
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace Capycom
{
	public class UpdateSessionMiddleware
	{
		private readonly RequestDelegate _next;
		private IServiceProvider _serviceProvider;

		public UpdateSessionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public UpdateSessionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
		{
			_next = next;
			_serviceProvider = serviceProvider;
		}

#if ASYNC
		public async Task InvokeAsync(HttpContext context)
		{

			if (context.Session.TryGetValue("MySessionVariable", out _))
			{
				await _next(context);
			}
			else
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var _context = scope.ServiceProvider.GetRequiredService<CapycomContext>();
					if (context.User.Identity.IsAuthenticated)
					{
						var user = _context.CpcmUsers.FirstOrDefault(u => u.CpcmUserId.ToString() == context.User.FindFirstValue("CpcmUserId"));
						if (user != null)
						{
							context.Session.SetString("ProfileImage", string.IsNullOrEmpty(user.CpcmUserImagePath) ? Path.Combine("\\", "images", "default.png") : user.CpcmUserImagePath);
						}
					}
				}

				await _next(context);
			}
			return;

		}
#else

		public Task InvokeAsync(HttpContext context)
		{
			return Task.Run(() =>
			{
				if (context.Session.TryGetValue("MySessionVariable", out _))
				{
					_next(context).Wait();
				}
				else
				{
					using (var scope = _serviceProvider.CreateScope())
					{
						var _context = scope.ServiceProvider.GetRequiredService<CapycomContext>();
						if (context.User.Identity.IsAuthenticated)
						{
							var user = _context.CpcmUsers.FirstOrDefault(u => u.CpcmUserId.ToString() == context.User.FindFirstValue("CpcmUserId"));
							if (user != null)
							{
								context.Session.SetString("ProfileImage", string.IsNullOrEmpty(user.CpcmUserImagePath) ? Path.Combine("\\", "images", "default.png") : user.CpcmUserImagePath);
							}
						}
					}

					_next(context).Wait();
				}
				return;
				
			});
		}
#endif
	}
}
