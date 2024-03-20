﻿#define AdminAutoAuth
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
namespace Capycom.Controllers
{
    public class UserLogInController : Controller
    {
        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<HomeController> _logger;

        public UserLogInController(ILogger<HomeController> logger, CapycomContext context, IOptions<MyConfig> config)
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
        public async Task<IActionResult> LogIn(UserLogInModel user)
        {
#if AdminAutoAuth
            if(true)
#else
if (ModelState.IsValid)
#endif

            {
#if AdminAutoAuth
                CpcmUser potentialUser = _context.CpcmUsers.Include(c => c.CpcmUserRoleNavigation).Where(e => e.CpcmUserEmail == "asdas@asd.ru").First();               
#else
                CpcmUser potentialUser = _context.CpcmUsers.Where(e => e.CpcmUserEmail == user.CpcmUserEmail).First();
#endif
                string potentialUserSalt = potentialUser.CpcmUserSalt;
#if AdminAutoAuth
                if (true)
#else
                if (potentialUser.CpcmUserPwdHash == MyConfig.GetSha256Hash(user.CpcmUserPwd, potentialUserSalt, _config.ServerSol))
#endif

                {
                    List<Claim> claims = GetUserClaims(potentialUser); 
                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    //var kek = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == "1");
                    //kek.Value;
                    return RedirectToAction("Test");
                }
                else
                {

                }
            }
            return View("Index",user);
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
                returnClaims.Add(new Claim(property.Name, property.GetValue(userRole).ToString()));                
            }
            return returnClaims;
        }
    }
}
