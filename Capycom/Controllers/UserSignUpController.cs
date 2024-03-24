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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserSignUpModel cpcmSignUser)
        {            

            if (ModelState.IsValid)
            {
                CpcmUser cpcmUser = new();
                cpcmUser.CpcmUserId = Guid.NewGuid();
                cpcmUser.CpcmUserEmail = cpcmSignUser.CpcmUserEmail.Trim();
                cpcmUser.CpcmUserTelNum = cpcmSignUser.CpcmUserTelNum.Trim();
                cpcmUser.CpcmUserSalt = MyConfig.GetRandomString(10);
                cpcmUser.CpcmUserPwdHash = MyConfig.GetSha256Hash(cpcmSignUser.CpcmUserPwd.Trim(),cpcmUser.CpcmUserSalt,_config.ServerSol);
                cpcmUser.CpcmUserAbout = cpcmSignUser.CpcmUserAbout?.Trim();
                cpcmUser.CpcmUserCity = cpcmSignUser.CpcmUserCity;
                cpcmUser.CpcmUserSite = cpcmSignUser.CpcmUserSite?.Trim();
                cpcmUser.CpcmUserBooks = cpcmSignUser.CpcmUserBooks?.Trim();
                cpcmUser.CpcmUserFilms = cpcmSignUser.CpcmUserFilms?.Trim();
                cpcmUser.CpcmUserMusics = cpcmSignUser.CpcmUserMusics?.Trim();
                cpcmUser.CpcmUserSchool = cpcmSignUser.CpcmUserSchool;
                cpcmUser.CpcmUserUniversity = cpcmSignUser.CpcmUserUniversity;
                //cpcmUser.CpcmUserImagePath = cpcmSignUser.CpcmUserImagePath;
                //cpcmUser.CpcmUserCoverPath = cpcmSignUser.CpcmUserCoverPath;

                cpcmSignUser.CpcmUserNickName = cpcmSignUser.CpcmUserNickName?.Trim();
                if (cpcmSignUser.CpcmUserNickName == "" || cpcmSignUser.CpcmUserNickName == null)
                {
                    cpcmUser.CpcmUserNickName = null;
                }
                else
                {
                    cpcmUser.CpcmUserNickName = cpcmSignUser.CpcmUserNickName;
                }

                cpcmUser.CpcmUserFirstName = cpcmSignUser.CpcmUserFirstName.Trim();
                cpcmUser.CpcmUserSecondName = cpcmSignUser.CpcmUserSecondName.Trim();
                cpcmUser.CpcmUserAdditionalName = cpcmSignUser.CpcmUserAdditionalName?.Trim();
                cpcmUser.CpcmUserRole = UserSignUpModel.BaseUserRole;

                string filePathUserImage = "";
                if(cpcmSignUser.CpcmUserImage != null && cpcmSignUser.CpcmUserImage.Length != 0)// Почему тут а не в [Remote] - чтобы клиент не посылал запросы дважды. Т.е. чтобы клиент не посылал запрос на валидацию, а потом всю форму. 
                {
                    CheckIFormFile("CpcmUserImage", cpcmSignUser.CpcmUserImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (ModelState.IsValid)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cpcmSignUser.CpcmUserImage.FileName);
                        filePathUserImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePathUserImage, FileMode.Create))
                            {
                                await cpcmSignUser.CpcmUserImage.CopyToAsync(fileStream);
                            }
                            cpcmUser.CpcmUserImagePath = filePathUserImage;
                        }
                        catch (Exception ex)
                        {
                            cpcmUser.CpcmUserImagePath = null;
                        }
                    }

                }

                string filePathUserCoverImage = "";
                if (cpcmSignUser.CpcmUserCoverImage != null && cpcmSignUser.CpcmUserCoverImage.Length != 0)
                {
                    CheckIFormFile("CpcmUserCoverImage", cpcmSignUser.CpcmUserCoverImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (ModelState.IsValid)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cpcmSignUser.CpcmUserCoverImage.FileName); //System.IO.Path.GetRandomFileName()
                        filePathUserCoverImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePathUserCoverImage, FileMode.Create))
                            {
                                await cpcmSignUser.CpcmUserCoverImage.CopyToAsync(fileStream);
                            }
                            cpcmUser.CpcmUserCoverPath = filePathUserCoverImage;
                        }
                        catch (Exception ex)
                        {
                            cpcmUser.CpcmUserCoverPath = null;
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", cpcmSignUser.CpcmUserCity);
                    ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", cpcmSignUser.CpcmUserSchool);
                    ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", cpcmSignUser.CpcmUserUniversity);
                    return View(cpcmSignUser);
                }


                _context.Add(cpcmUser);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(filePathUserImage))
                    {
                        System.IO.File.Delete(filePathUserImage);
                    }
                    if (System.IO.File.Exists(filePathUserCoverImage))
                    {
                        System.IO.File.Delete(filePathUserCoverImage);
                    }
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
            if(string.IsNullOrWhiteSpace(CpcmUserEmail))
            {
                return Json("Email не может быть пустым или состоять из одних пробелов");
            }
            CpcmUserEmail = CpcmUserEmail.Trim();
            if (CpcmUserEmail.Contains("admin") || CpcmUserEmail.Contains("webmaster") || CpcmUserEmail.Contains("abuse"))
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserEmail == CpcmUserEmail));
        }

        [HttpPost]
        public async Task<IActionResult> CheckNickName(string CpcmUserNickName)
         {
            if (CpcmUserNickName == null || CpcmUserNickName.All(char.IsWhiteSpace) || CpcmUserNickName==string.Empty)
            {
                return Json(true);
            }
            CpcmUserNickName = CpcmUserNickName.Trim(); 
            if (CpcmUserNickName.Contains("admin") || CpcmUserNickName.Contains("webmaster") || CpcmUserNickName.Contains("abuse") || CpcmUserNickName.Contains(" "))
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserNickName == CpcmUserNickName));
        }

        [HttpPost]
        public async Task<IActionResult> CheckPhone(string CpcmUserTelNum)
        {
            if (string.IsNullOrWhiteSpace(CpcmUserTelNum))
            {
                return Json("Телефон не может быть пустым или состоять из одних пробелов");
            }
            CpcmUserTelNum = CpcmUserTelNum.Trim();
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserTelNum == CpcmUserTelNum));
        }

        [HttpPost]
        public async Task<IActionResult> AddCity(string newCity)
        {
            if (!string.IsNullOrWhiteSpace(newCity) && !_context.CpcmCities.Any(e => e.CpcmCityName == newCity.Trim()))
            {
                CpcmCity city = new();
                city.CpcmCityId = Guid.NewGuid();
                newCity = newCity.Trim();
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
            if (!string.IsNullOrWhiteSpace(newSchool) && !_context.CpcmSchools.Any(e => e.CpcmSchoolName == newSchool.Trim()))
            {
                CpcmSchool school= new();
                school.CpcmSchooldId = Guid.NewGuid();
                newSchool = newSchool.Trim();
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
            if (!string.IsNullOrWhiteSpace(newUni) && !_context.CpcmUniversities.Any(e => e.CpcmUniversityName == newUni.Trim()))
            {
                CpcmUniversity university= new();
                university.CpcmUniversityId = Guid.NewGuid();
                newUni = newUni.Trim();
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
        private bool CheckIFormFileContent(IFormFile cpcmUserImage, string[] permittedTypes)
        {
            if (cpcmUserImage != null && permittedTypes.Contains(cpcmUserImage.ContentType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckIFormFileSize(IFormFile cpcmUserImage, int size)
        {

            if (cpcmUserImage.Length > 0 && cpcmUserImage.Length < size)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CheckIFormFile(string FormFieldName, IFormFile file, int size, string[] permittedTypes)
        {
            bool status = true;
            if (!CheckIFormFileContent(file,permittedTypes))
            {
                ModelState.AddModelError(FormFieldName, "Допустимые типы файлов: png, jpeg, jpg, gif");
                status = false;
            }
            if (!CheckIFormFileSize(file, size))
            {
                ModelState.AddModelError(FormFieldName, $"Максимальный размер файла: {size/1024} Кбайт");
                status = false;
            }
            return status;
        }

    }
}
