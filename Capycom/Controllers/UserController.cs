using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Plugins;

namespace Capycom.Controllers
{
    public class UserController : Controller
    {

        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<HomeController> _logger;

        public UserController(ILogger<HomeController> logger, CapycomContext context, IOptions<MyConfig> config)
        {
            _context = context;
            _config = config.Value;
            _logger = logger;
        }



        [Authorize]
        public async Task<ActionResult> Index()
        {
            string userId = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value;

            //CpcmUser user = _context.CpcmUsers
            //    .Include(c => c.CpcmUserCityNavigation)
            //    .Include(c => c.CpcmUserRoleNavigation)
            //    .Include(c => c.CpcmUserSchoolNavigation)
            //    .Include(c => c.CpcmUserUniversityNavigation)
            //    .Where(c => c.CpcmUserId == Guid.Parse(userId)).First();

            //return View(user);

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(userId)).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user.CpcmUserNickName != null)
            {
                return RedirectToAction("Index", new { nickName = user.CpcmUserNickName });
            }
            else
            {
                return RedirectToAction("Index", new { id = user.CpcmUserId });
            }

        }

        public async Task<ActionResult> Index(Guid id)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers
                .Include(c => c.CpcmUserCityNavigation)
                .Include(c => c.CpcmUserRoleNavigation)
                .Include(c => c.CpcmUserSchoolNavigation)
                .Include(c => c.CpcmUserUniversityNavigation)
                .Where(c => c.CpcmUserId == id).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }
            if(user.CpcmUserNickName != null)
            {
                return RedirectToAction("Index", new { nickName = user.CpcmUserNickName });
            }
            return View(user);
        }

        [Route("User/{nickName}")]
        public async Task<ActionResult> Index(string nickName)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers
                .Include(c => c.CpcmUserCityNavigation)
                .Include(c => c.CpcmUserRoleNavigation)
                .Include(c => c.CpcmUserSchoolNavigation)
                .Include(c => c.CpcmUserUniversityNavigation)
                .Where(c => c.CpcmUserNickName == nickName).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }
            return View(user);
        }



        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Edit(string id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                return View("Error418");
            }

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }


            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            return View(user);

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserEditAboutDataModel user)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", user.CpcmUserId))
            {
                return StatusCode(403);
            }

            if (ModelState.IsValid)
            {
                CpcmUser cpcmUser;
                try
                {
                    cpcmUser = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(user.CpcmUserId.ToString())).FirstAsync();
                }
                catch (Exception)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("Error418");
                }

                cpcmUser.CpcmUserAbout = user.CpcmUserAbout;
                cpcmUser.CpcmUserCity = user.CpcmUserCity;
                cpcmUser.CpcmUserSite = user.CpcmUserSite;
                cpcmUser.CpcmUserBooks = user.CpcmUserBooks;
                cpcmUser.CpcmUserFilms = user.CpcmUserFilms;
                cpcmUser.CpcmUserMusics = user.CpcmUserMusics;
                cpcmUser.CpcmUserSchool = user.CpcmUserSchool;
                cpcmUser.CpcmUserUniversity = user.CpcmUserUniversity;


                cpcmUser.CpcmUserFirstName = user.CpcmUserFirstName;
                cpcmUser.CpcmUserSecondName = user.CpcmUserSecondName;
                cpcmUser.CpcmUserAdditionalName = user.CpcmUserAdditionalName;


                string filePathUserImage = "";
                if (user.CpcmUserImage != null && user.CpcmUserImage.Length != 0)// Почему тут а не в [Remote] - чтобы клиент не посылал запросы дважды. Т.е. чтобы клиент не посылал запрос на валидацию, а потом всю форму. 
                {
                    CheckIFormFile("CpcmUserImage", user.CpcmUserImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (ModelState.IsValid)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(user.CpcmUserImage.FileName);
                        filePathUserImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePathUserImage, FileMode.Create))
                            {
                                await user.CpcmUserImage.CopyToAsync(fileStream);
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
                if (user.CpcmUserCoverImage != null && user.CpcmUserCoverImage.Length != 0)
                {
                    CheckIFormFile("CpcmUserCoverImage", user.CpcmUserCoverImage, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (ModelState.IsValid)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(user.CpcmUserCoverImage.FileName); //System.IO.Path.GetRandomFileName()
                        filePathUserCoverImage = Path.Combine("wwwroot", "uploads", uniqueFileName);

                        try
                        {
                            using (var fileStream = new FileStream(filePathUserCoverImage, FileMode.Create))
                            {
                                await user.CpcmUserCoverImage.CopyToAsync(fileStream);
                            }
                            cpcmUser.CpcmUserCoverPath = filePathUserCoverImage;
                        }
                        catch (Exception ex)
                        {
                            cpcmUser.CpcmUserCoverPath = null;
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
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

                if (cpcmUser.CpcmUserNickName != null)
                {
                    return RedirectToAction("Index", new { nickName = cpcmUser.CpcmUserNickName });
                }
                else
                {
                    return RedirectToAction("Index", new { id = cpcmUser.CpcmUserId });
                }
            }

            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> EditIdentity(string id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                return View("Error418");
            }
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }
            return View(user);

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditIdentity(UserEditIdentityModel user)
        {           
            if (ModelState.IsValid)
            {
                if (!CheckUserPrivilege("CpcmCanEditUsers", "True", user.CpcmUserId))
                {
                    return StatusCode(403);
                }


                CpcmUser cpcmUser;
                try
                {
                    cpcmUser = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(user.CpcmUserId.ToString())).FirstAsync();
                }
                catch (Exception)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("Error418");
                }

                cpcmUser.CpcmUserEmail = user.CpcmUserEmail;
                cpcmUser.CpcmUserTelNum = user.CpcmUserTelNum;
                cpcmUser.CpcmUserNickName = user.CpcmUserNickName;
                cpcmUser.CpcmUserPwdHash = MyConfig.GetSha256Hash(user.CpcmUserPwd, cpcmUser.CpcmUserSalt, _config.ServerSol);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Не удалось сохранить вас как нового пользователя. Возможно вы указали данные, которые не поддерживаются нами. Обратитесь в техническую поддержку";
                    return View("Error418");
                }


                return RedirectToAction($"Index\\{user.CpcmUserId}");
            }

            return View(user);
        }



        public async Task<ActionResult> Friends(Guid id)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }


            if (user.CpcmUserNickName != null)
            {
                return RedirectToAction("Friends", new { nickName = user.CpcmUserNickName });
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> friendList1;
            List<CpcmUser> friendList2;
            try
            {
                friendList1 = await _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus==true).Select(c => c.CmcpFriend).ToListAsync();
                friendList2 = await _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            friendList1.AddRange(friendList2);

            return View(friendList1);
        }

        [Route("User/Friends/{nickName}")]
        public async Task<ActionResult> Friends(string nickName)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> friendList1;
            List<CpcmUser> friendList2;
            try
            {
                friendList1 = await _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpFriend).ToListAsync();
                friendList2 = await _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            friendList1.AddRange(friendList2);

            return View(friendList1);
        }

        public async Task<ActionResult> Followers(Guid id)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }


            if (user.CpcmUserNickName != null)
            {
                return RedirectToAction("Followers", new { nickName = user.CpcmUserNickName });
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> followerList1;
            List<CpcmUser> followerList2;
            try
            {
                followerList1 = await _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmFollower).ToListAsync();
                followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            followerList1.AddRange(followerList2);

            return View(followerList1);
        }

        [Route("User/Followers/{nickName}")]
        public async Task<ActionResult> Followers(string nickName)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> followerList1;
            List<CpcmUser> followerList2;
            try
            {
                followerList1 = await _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmFollower).ToListAsync();
                followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            followerList1.AddRange(followerList2);

            return View(followerList1);
        }



        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }







        private bool CheckUserPrivilege(string claimType, string claimValue, string id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id || c.Type == "claimType" && c.Value == "claimValue");
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }
        private bool CheckUserPrivilege(string claimType, string claimValue, Guid id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id.ToString() || c.Type == "claimType" && c.Value == "claimValue");
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }

        private bool CheckIFormFileContent(IFormFile cpcmUserImage, string[] permittedTypes)//TODO: Объединить с методами при регистрации
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
        private bool CheckIFormFileSize(IFormFile cpcmUserImage, int size)//TODO: Объединить с методами при регистрации
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
        private bool CheckIFormFile(string FormFieldName, IFormFile file, int size, string[] permittedTypes)//TODO: Объединить с методами при регистрации
        {
            bool status = true;
            if (!CheckIFormFileContent(file, permittedTypes))
            {
                ModelState.AddModelError(FormFieldName, "Допустимые типы файлов: png, jpeg, jpg, gif");
                status = false;
            }
            if (!CheckIFormFileSize(file, size))
            {
                ModelState.AddModelError(FormFieldName, $"Максимальный размер файла: {size / 1024} Кбайт");
                status = false;
            }
            return status;
        }

        [HttpPost] //TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckEmail(string CpcmUserEmail)
        {
            if (CpcmUserEmail.Contains("admin") || CpcmUserEmail.Contains("webmaster") || CpcmUserEmail.Contains("abuse")&& !HttpContext.User.Identity.IsAuthenticated)
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserEmail == CpcmUserEmail && e.CpcmUserId.ToString()!=HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value));
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckNickName(string CpcmUserNickName)
        {
            if (CpcmUserNickName.Contains("admin") || CpcmUserNickName.Contains("webmaster") || CpcmUserNickName.Contains("abuse") && !HttpContext.User.Identity.IsAuthenticated)
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserNickName == CpcmUserNickName && e.CpcmUserId.ToString() != HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value));
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckPhone(string CpcmUserTelNum)
        {
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserTelNum == CpcmUserTelNum && e.CpcmUserId.ToString() != HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value));
        }


    }
}
