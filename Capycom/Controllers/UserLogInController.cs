//#define AdminAutoAuth
using Microsoft.AspNetCore.Mvc;
using Capycom.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
namespace Capycom.Controllers
{
    public class UserLogInController : Controller
    {
        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<UserLogInController> _logger;

        public UserLogInController(ILogger<UserLogInController> logger, CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserLogInModel user)
        {
            CpcmUser? potentialUser;
            if(HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "User");
#if AdminAutoAuth
            if(true)
#else
if (ModelState.IsValid)
#endif

            {
#if AdminAutoAuth
                 potentialUser = await _context.CpcmUsers.Include(c => c.CpcmUserRoleNavigation).Where(e => e.CpcmUserEmail == "asdas@asd.ru").FirstAsync();               
#else
                try
                {
                    potentialUser = await _context.CpcmUsers.Where(e => e.CpcmUserEmail == user.CpcmUserEmail.Trim()).Include(p => p.CpcmUserRoleNavigation).FirstOrDefaultAsync();
                    if(potentialUser == null)
                    {
                        ViewData["Message"] = "Неверный логин или пароль";
                        return View();
                    }
#endif
                }
                catch (DbException)
                {
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Ошибка связи с сервером";
                    return View("UserError");
                }
                string potentialUserSalt = potentialUser.CpcmUserSalt;
#if AdminAutoAuth
                if (true)
#else
                //if (potentialUser.CpcmUserPwdHash == MyConfig.GetSha256Hash(user.CpcmUserPwd.Trim(), potentialUserSalt, _config.ServerSol))
                if (potentialUser.CpcmUserPwdHash.SequenceEqual(MyConfig.GetSha256Hash(user.CpcmUserPwd.Trim(), potentialUserSalt, _config.ServerSol)))
#endif

					{
                    if(potentialUser.CpcmUserBanned == true)
                    {
                        Response.StatusCode = 403;
                        ViewData["ErrorCode"] = 403;
                        ViewData["Message"] = "Вы забанены за нарушение условия пользования Capycom. Если вы считаете, что банхаммер прилетел неправомерно - обратитесь в администрацию";
                        return View("UserError");
                    }
                    if (potentialUser.CpcmIsDeleted == true)
                    {
                        //Response.StatusCode = 404;
                        //ViewData["ErrorCode"] = 404;
                        //ViewData["Message"] = "Аккаунт был удалён";
                        //return View("UserError");
                        ViewData["Message"] = "Неверный логин или пароль";
                        return View();
                    }
                    List<Claim> claims = GetUserClaims(potentialUser); 
                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    //var kek = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == "1");
                    //kek.Value;
                    return RedirectToAction("Test"); // TODO махнкть на юзер индекс
                }
                else
                {
                    ViewData["Message"] = "Неверный логин или пароль";
                    return View();
                }
            }
            return View("Index");
        }

        [Authorize]
        public async Task<IActionResult> Test()
        {
            IEnumerable<Claim> a = HttpContext.User.Claims;
            return View("Error418");
        }

        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        private List<Claim> GetUserClaims(CpcmUser user)
        {
            List<Claim> returnClaims = new List<Claim> { new Claim("CpcmUserId", user.CpcmUserId.ToString()) };

            CpcmRole userRole = user.CpcmUserRoleNavigation;
            Type type = userRole.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
           
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(string))
                {
                    returnClaims.Add(new Claim(property.Name, property.GetValue(userRole).ToString()));          
                }       
            }
            return returnClaims;
        }
    }
}
