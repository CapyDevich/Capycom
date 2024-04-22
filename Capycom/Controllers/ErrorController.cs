using Capycom.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace Capycom.Controllers
{
    public class ErrorController : Controller
	{

		public async Task<IActionResult> Error(ErrorModel errorModel)
		{
			if (errorModel != null)
			{
				Response.StatusCode = errorModel.StatusCode;
				return View("UserError",errorModel);
			}
			Response.StatusCode = 500;
			return View("UserError",errorModel);
		}

		public async Task<IActionResult> Code403()
		{
			return StatusCode(403);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public async Task<IActionResult> ErrorF()
		{
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			Log.Fatal(exceptionHandlerPathFeature.Error, "Неперехваченное исключение произошло. {@0}", exceptionHandlerPathFeature);
			ViewData["Message"] = "Пожалуйста, повторите запрос спустя некоторое время. Если ошибка продолжает появляться - свяжитель с администрацией";
			return View("UserError");
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

	}
}
