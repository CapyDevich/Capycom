using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capycom;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Capycom.Models;


namespace Capycom.Controllers
{
    public class UserSignUpController : Controller
    {
        private readonly CapycomContext _context;
        private readonly MyConfig _config;

        public UserSignUpController(CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        // GET: UserSignUp
        public async Task<IActionResult> Index()
        {
            var capycomContext = _context.CpcmUsers.Include(c => c.CpcmUserCityNavigation).Include(c => c.CpcmUserRoleNavigation).Include(c => c.CpcmUserSchoolNavigation).Include(c => c.CpcmUserUniversityNavigation);
            return View(await capycomContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityId");
            ViewData["CpcmUserRole"] = new SelectList(_context.CpcmRoles, "CpcmRoleId", "CpcmRoleId");
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchooldId");
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityId");
            return View();
        }

        // POST: UserSignUp/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserSignUpModel cpcmSignUser)
        {            

            if (ModelState.IsValid)
            {
                CpcmUser cpcmUser = new();
                cpcmUser.CpcmUserId = Guid.NewGuid();
                cpcmUser.CpcmUserEmail = cpcmSignUser.CpcmUserEmail;
                cpcmUser.CpcmUserTelNum = cpcmSignUser.CpcmUserTelNum;
                cpcmUser.CpcmUserSalt = GetRandomString(10);
                cpcmUser.CpcmUserPwdHash = GetSha256Hash(cpcmSignUser.CpcmUserPwd,cpcmUser.CpcmUserSalt,_config.ServerSol);
                cpcmUser.CpcmUserAbout = cpcmSignUser.CpcmUserAbout;
                cpcmUser.CpcmUserCity = cpcmSignUser.CpcmUserCity;
                cpcmUser.CpcmUserSite = cpcmSignUser.CpcmUserSite;
                cpcmUser.CpcmUserBooks = cpcmSignUser.CpcmUserBooks;
                cpcmUser.CpcmUserFilms = cpcmSignUser.CpcmUserFilms;
                cpcmUser.CpcmUserMusics = cpcmSignUser.CpcmUserMusics;
                cpcmUser.CpcmUserSchool = cpcmSignUser.CpcmUserSchool;
                cpcmUser.CpcmUserUniversity = cpcmSignUser.CpcmUserUniversity;
                cpcmUser.CpcmUserImagePath = cpcmSignUser.CpcmUserImagePath;
                cpcmUser.CpcmUserCoverPath = cpcmSignUser.CpcmUserCoverPath;
                cpcmUser.CpcmUserNickName = cpcmSignUser.CpcmUserNickName;
                cpcmUser.CpcmUserFirstName = cpcmSignUser.CpcmUserFirstName;
                cpcmUser.CpcmUserSecondName = cpcmSignUser.CpcmUserSecondName;
                cpcmUser.CpcmUserAdditionalName = cpcmSignUser.CpcmUserAdditionalName;
                cpcmUser.CpcmUserRole = UserSignUpModel.BaseUserRole; 

                _context.Add(cpcmUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
            return View(cpcmSignUser);
            //return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmail(string CpcmUserEmail)
        {
            if (CpcmUserEmail.Contains("admin") || CpcmUserEmail.Contains("webmaster") || CpcmUserEmail.Contains("abuse"))
            {
                return Json(false);
            }
            return Json(!_context.CpcmUsers.Any(e => e.CpcmUserEmail == CpcmUserEmail));
        }

        [HttpPost]
        public async Task<IActionResult> CheckNickName(string CpcmUserNickName)
        {
            if (CpcmUserNickName.Contains("admin") || CpcmUserNickName.Contains("webmaster") || CpcmUserNickName.Contains("abuse"))
            {
                return Json(false);
            }
            return Json(!_context.CpcmUsers.Any(e => e.CpcmUserNickName == CpcmUserNickName));
        }

        [HttpPost]
        public async Task<IActionResult> CheckPhone(string CpcmUserTelNum)
        {
            return Json(!_context.CpcmUsers.Any(e => e.CpcmUserTelNum == CpcmUserTelNum));
        }

        private bool CpcmUserExists(Guid id)
        {
            return _context.CpcmUsers.Any(e => e.CpcmUserId == id);
        }
        private static byte[] GetSha256Hash(string stringToSHA, string sol, string serversol)
        {
            if (stringToSHA == null || stringToSHA == String.Empty)
            {
                throw new ArgumentException("Строка была пустой или null");
            }

            byte[] returnValue;
            returnValue = SHA256.HashData(Encoding.Unicode.GetBytes(stringToSHA+sol+serversol));
            return returnValue;
        }
        private static string GetRandomString(int length)
        {
            Random rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
