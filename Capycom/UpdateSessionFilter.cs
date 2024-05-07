using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Capycom
{
	public class UpdateSessionFilter : IActionFilter
	{
		private readonly CapycomContext _dbContext;

		public UpdateSessionFilter(CapycomContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
			
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{

			if (context.HttpContext.User.Identity.IsAuthenticated)
			{
				var user = _dbContext.CpcmUsers.FirstOrDefault(u => u.CpcmUserId.ToString() == context.HttpContext.User.FindFirst("CpcmUserId").Value);
				if (user != null)
				{
					context.HttpContext.Session.SetString("ProfileImage", string.IsNullOrEmpty(user.CpcmUserImagePath) ? Path.Combine("wwwroot", "images", "default.png").Replace("wwwroot", "") : user.CpcmUserImagePath);
				}
			}

		}
	}
}
