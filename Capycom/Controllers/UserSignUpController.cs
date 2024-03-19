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
        private readonly ILogger<HomeController> _logger;
        private readonly MyConfig _config;

        public UserSignUpController(ILogger<HomeController> logger, CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
            _logger = logger;
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
        //[RequestSizeLimit(10240)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserSignUpModel cpcmSignUser)
        {            

            if (ModelState.IsValid)
            {
                CpcmUser cpcmUser = new();
                cpcmUser.CpcmUserId = Guid.NewGuid();
                cpcmUser.CpcmUserEmail = cpcmSignUser.CpcmUserEmail;
                cpcmUser.CpcmUserTelNum = cpcmSignUser.CpcmUserTelNum;
                cpcmUser.CpcmUserSalt = MyConfig.GetRandomString(10);
                cpcmUser.CpcmUserPwdHash = MyConfig.GetSha256Hash(cpcmSignUser.CpcmUserPwd,cpcmUser.CpcmUserSalt,_config.ServerSol);
                cpcmUser.CpcmUserAbout = cpcmSignUser.CpcmUserAbout;
                cpcmUser.CpcmUserCity = cpcmSignUser.CpcmUserCity;
                cpcmUser.CpcmUserSite = cpcmSignUser.CpcmUserSite;
                cpcmUser.CpcmUserBooks = cpcmSignUser.CpcmUserBooks;
                cpcmUser.CpcmUserFilms = cpcmSignUser.CpcmUserFilms;
                cpcmUser.CpcmUserMusics = cpcmSignUser.CpcmUserMusics;
                cpcmUser.CpcmUserSchool = cpcmSignUser.CpcmUserSchool;
                cpcmUser.CpcmUserUniversity = cpcmSignUser.CpcmUserUniversity;
                //cpcmUser.CpcmUserImagePath = cpcmSignUser.CpcmUserImagePath;
                //cpcmUser.CpcmUserCoverPath = cpcmSignUser.CpcmUserCoverPath;
                cpcmUser.CpcmUserNickName = cpcmSignUser.CpcmUserNickName;
                cpcmUser.CpcmUserFirstName = cpcmSignUser.CpcmUserFirstName;
                cpcmUser.CpcmUserSecondName = cpcmSignUser.CpcmUserSecondName;
                cpcmUser.CpcmUserAdditionalName = cpcmSignUser.CpcmUserAdditionalName;
                cpcmUser.CpcmUserRole = UserSignUpModel.BaseUserRole; 

                if(cpcmSignUser.CpcmUserImage != null && cpcmSignUser.CpcmUserImage.Length != 0)// Почему тут а не в [Remote] - чтобы клиент не посылал запросы дважды. Т.е. чтобы клиент не посылал запрос на валидацию, а потом всю форму. 
                {
                    if (!CheckIFileContent(cpcmSignUser.CpcmUserImage))
                    {
                        ModelState.AddModelError("CpcmUserImage", "Допустимые типы файлов: png, jpeg, gif");
                        ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
                        ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
                        ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
                        return View(cpcmSignUser);
                    }
                    if (!CheckIFileSize(cpcmSignUser.CpcmUserImage))
                    {
                        ModelState.AddModelError("CpcmUserImage", "Размер файла превышает 8Мбайт");
                        ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
                        ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
                        ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
                        return View(cpcmSignUser);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cpcmSignUser.CpcmUserImage.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await cpcmSignUser.CpcmUserImage.CopyToAsync(fileStream);
                    }
                    cpcmUser.CpcmUserImagePath = filePath;

                }
                
                if (cpcmSignUser.CpcmUserCoverImage != null && cpcmSignUser.CpcmUserCoverImage.Length != 0)
                {

                    if (!CheckIFileContent(cpcmSignUser.CpcmUserCoverImage))
                    {
                        ModelState.AddModelError("CpcmUserCoverImage", "Допустимые типы файлов: png, jpeg, gif");
                        ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
                        ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
                        ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
                        return View(cpcmSignUser);
                    }
                    if (!CheckIFileSize(cpcmSignUser.CpcmUserCoverImage))
                    {
                        ModelState.AddModelError("CpcmUserCoverImage", "Размер файла превышает 8Мбайт");
                        ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
                        ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
                        ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
                        return View(cpcmSignUser);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cpcmSignUser.CpcmUserCoverImage.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await cpcmSignUser.CpcmUserCoverImage.CopyToAsync(fileStream);
                    }
                    cpcmUser.CpcmUserCoverPath = filePath;

                }



                _context.Add(cpcmUser);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Не удалось сохранить вас как нового пользователя. Возможно вы указали данные, которые не поддерживаются нами. Обратитесь в техническую поддержку";
                    return View("Error418");
                }
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

        [HttpPost]
        public async Task<IActionResult> AddCity(string newCity)
        {
            if (!string.IsNullOrEmpty(newCity) && !_context.CpcmCities.Any(e => e.CpcmCityName == newCity))
            {
                CpcmCity city = new();
                city.CpcmCityId = Guid.NewGuid();
                city.CpcmCityName = newCity;

                _context.CpcmCities.Add(city);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> AddSchool(string newSchool)
        {
            if (!string.IsNullOrEmpty(newSchool) && !_context.CpcmSchools.Any(e => e.CpcmSchoolName == newSchool))
            {
                CpcmSchool school= new();
                school.CpcmSchooldId = Guid.NewGuid();
                school.CpcmSchoolName = newSchool;

                _context.CpcmSchools.Add(school);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> AddUniversities(string newUni)
        {
            if (!string.IsNullOrEmpty(newUni) && !_context.CpcmUniversities.Any(e => e.CpcmUniversityName == newUni))
            {
                CpcmUniversity university= new();
                university.CpcmUniversityId = Guid.NewGuid();
                university.CpcmUniversityName = newUni;

                _context.CpcmUniversities.Add(university);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
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
        private bool CheckIFileContent(IFormFile cpcmUserImage)
        {
            var permittedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (cpcmUserImage != null && permittedTypes.Contains(cpcmUserImage.ContentType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckIFileSize(IFormFile cpcmUserImage)
        {

            if (cpcmUserImage.Length > 0 && cpcmUserImage.Length < 8192)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
