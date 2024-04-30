using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Capycom.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime dateTime)
            {
				var httpContextAccessor = (IHttpContextAccessor)validationContext.GetService(typeof(IHttpContextAccessor));
				if (httpContextAccessor.HttpContext.Request.Cookies.ContainsKey("TimeZone"))
				{
					string timezoneOffsetCookie = httpContextAccessor.HttpContext.Request.Cookies["TimeZone"];
					if (timezoneOffsetCookie != null)
					{
						if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
						{
							TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

							if (offset.TotalHours <= 24)
							{
								if (dateTime + offset < DateTime.UtcNow )
								{
									return new ValidationResult("Дата должна быть больше или равна текущей дате", new List<string> { validationContext.DisplayName });
								}
							}
							else
							{
								if (dateTime < DateTime.UtcNow)
								{
									return new ValidationResult("Неккоректная дата. Очистите куки TimeZone и перезагрузите страницу", new List<string> { validationContext.DisplayName });
								}
							}

						}
						else
						{
							return new ValidationResult("Неккоректная дата. Очистите куки TimeZone и перезагрузите страницу", new List<string> { validationContext.DisplayName });
						}
					}
					else
					{
						return new ValidationResult("Неккоректная дата. Очистите куки TimeZone и перезагрузите страницу", new List<string> { validationContext.DisplayName });
					}
				}
				else
				{
					if (dateTime < DateTime.UtcNow)
					{
						return new ValidationResult("Неккоректная дата. Очистите куки TimeZone и перезагрузите страницу", new List<string> { validationContext.DisplayName });
					}
				}
				
            }

            return ValidationResult.Success;
        }
    }
}
