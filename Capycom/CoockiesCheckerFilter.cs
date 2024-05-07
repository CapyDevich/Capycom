using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Capycom
{
	public class CoockiesCheckerFilter : IActionFilter
	{

		public CoockiesCheckerFilter()
		{
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{

		}

		public void OnActionExecuting(ActionExecutingContext context)
		{

			if (!context.HttpContext.Request.Cookies.ContainsKey("TimeZone"))
			{
				//context.Result = new RedirectToActionResult("Index", "User", null);
				context.HttpContext.Response.Cookies.Append("TimeZone", "0");
				context.Result = new RedirectToActionResult("Index", "Home", null);
				return;
			}

			string timezoneOffsetCookie = context.HttpContext.Request.Cookies["TimeZone"];
			if (timezoneOffsetCookie != null)
			{
				if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
				{
					TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

					if (offset.TotalHours > 24)
					{
						context.Result = new RedirectToActionResult("Error", "ErrorWM", "Зачем вы редактировали куки? Верните как было.");
						return;
					}
					else
					{
						return;
					}

				}
			}
		}
	}
}
