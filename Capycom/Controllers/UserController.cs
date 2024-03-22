﻿using Capycom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Plugins;
using System.Data.Common;

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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(userId)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
                .Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }

            if (user.CpcmUserNickName != null)
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
                .Where(c => c.CpcmUserNickName == nickName).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
                    cpcmUser = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(user.CpcmUserId.ToString())).FirstOrDefaultAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("Error418");
                }

                if (user == null)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Пользователь не найден";
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

                if (!ModelState.IsValid)
                {
                    ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
                    ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
                    ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
                    return View(user);
                }


                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }
            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
                    cpcmUser = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(user.CpcmUserId.ToString())).FirstOrDefaultAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("Error418");
                }

                if (user == null)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Пользователь не найден";
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
                catch (DbException)
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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
            catch (DbException)
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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
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
            catch (DbException)
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
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }

            if (user.CpcmUserNickName != null)
            {
                return RedirectToAction("Followers", new { nickName = user.CpcmUserNickName });
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> followerList1;
            //List<CpcmUser> followerList2;
            try
            {
                followerList1 = await _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmFollower).ToListAsync();
                //followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            //followerList1.AddRange(followerList2);

            return View(followerList1);
        }

        [Route("User/Followers/{nickName}")]
        public async Task<ActionResult> Followers(string nickName)
        {
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            List<CpcmUser> followerList1;
            //List<CpcmUser> followerList2;
            try
            {
                followerList1 = await _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmFollower).ToListAsync();
                //followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            //followerList1.AddRange(followerList2);

            return View(followerList1);
        }



        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                return StatusCode(403);
            }

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(UserDeleteModel userdel)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", userdel.CpcmUserId))
            {
                return StatusCode(403);
            }

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == userdel.CpcmUserId).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("Error418");
            }

            if (user == null)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Пользователь не найден";
                return View("Error418");
            }

            _context.CpcmUsers.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 418;
                ViewData["Message"] = "Не удалось сохранить вас как нового пользователя. Возможно вы указали данные, которые не поддерживаются нами. Обратитесь в техническую поддержку";
                return View("Error418");
            }

            return RedirectToAction("UserLogIn", "Index");

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
        public async Task<IActionResult> CheckEmail(string CpcmUserEmail, Guid CpcmUserId)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmCanEditUsers" && c.Value == "True");
            if(authFactor != null)
            {
                return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserEmail == CpcmUserEmail && e.CpcmUserId!=CpcmUserId));
            }


            if (CpcmUserEmail.Contains("admin") || CpcmUserEmail.Contains("webmaster") || CpcmUserEmail.Contains("abuse"))
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserEmail == CpcmUserEmail && e.CpcmUserId != CpcmUserId));
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckNickName(string CpcmUserNickName, Guid CpcmUserId)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmCanEditUsers" && c.Value == "True");
            if (authFactor != null)
            {
                return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserNickName == CpcmUserNickName && e.CpcmUserId != CpcmUserId));
            }

            if (CpcmUserNickName.Contains("admin") || CpcmUserNickName.Contains("webmaster") || CpcmUserNickName.Contains("abuse"))
            {
                return Json(false);
            }
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserNickName == CpcmUserNickName && e.CpcmUserId != CpcmUserId));
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckPhone(string CpcmUserTelNum, Guid CpcmUserId)
        {
            return Json(!await _context.CpcmUsers.AnyAsync(e => e.CpcmUserTelNum == CpcmUserTelNum && e.CpcmUserId != CpcmUserId));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(Guid CpcmUserId)
        {

            CpcmUserfollower follower = new();
            follower.CpcmFollowerId = CpcmUserId;
            follower.CpcmUserId = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            _context.CpcmUserfollowers.Add(follower);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }

            return StatusCode(200);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(Guid CpcmUserId)
        {
            CpcmUserfollower? follow;
            try
            {
                follow = await _context.CpcmUserfollowers.Where(e => e.CpcmUserId == CpcmUserId).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }


            if(follow == null)
            {
                return StatusCode(400);
            }

            _context.Remove(follow);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }

            return StatusCode(200);

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFriendRequest(Guid CpcmUserId)
        {
            CpcmUserfriend friendRequest = new();
            friendRequest.CmcpUserId = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            friendRequest.CmcpFriendId = CpcmUserId;

            _context.CpcmUserfriends.Add(friendRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }

            return StatusCode(200);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerToFriendRequests(Guid CpcmUserId, bool status)
        {
            var friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value
            && c.CmcpFriendId == CpcmUserId).FirstOrDefaultAsync();

            friendRequest.CpcmFriendRequestStatus  = status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }

            return StatusCode(200);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteToFriendRequests(Guid CpcmUserId)
        {
            var friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value
            && c.CmcpFriendId == CpcmUserId).FirstOrDefaultAsync();

            _context.CpcmUserfriends.Remove(friendRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(400);
            }

            return StatusCode(200);
        }

        [Authorize]
        public async Task<IActionResult> CreatePost(Guid author)
        {
            return View(author);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(UserPostModel userPost)
        {

            if(ModelState.IsValid)
            {
                CpcmPost post = new CpcmPost();

                post.CpcmPostText = userPost.Text;
                post.CpcmPostId = Guid.NewGuid();
                post.CpcmPostFather = userPost.PostFatherId;
                post.CpcmPostCreationDate = DateTime.Now;
                post.CpcmPostPublishedDate = userPost.Published;

                List<string>filePaths = new List<string>(); 
                List<CpcmImage> images = new List<CpcmImage>();

                int i = 0;
                foreach(IFormFile file in userPost.Files)
                {
                    CheckIFormFile("Files", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (!ModelState.IsValid)
                    {
                       return View(userPost);

                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    filePaths.Add(Path.Combine("wwwroot", "uploads", uniqueFileName));

                    try
                    {
                        using (var fileStream = new FileStream(filePaths.Last(), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        CpcmImage image = new CpcmImage();
                        image.CpcmImageId = Guid.NewGuid();
                        image.CpcmPostId = post.CpcmPostId;
                        image.CpcmImagePath = filePaths.Last();
                        image.CpcmImageOrder = 0;
                        i++;

                        images.Add(image);
                    }
                    catch (Exception ex)
                    {
                        Response.StatusCode = 418;
                        ViewData["Message"] = "Не удалось сохранить фотографию на сервере. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                        return View(userPost);
                    }

                }
                post.CpcmImages = images;

                _context.CpcmPosts.Add(post);
                _context.CpcmImages.AddRange(images);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                    return View(userPost); // TODO Продумать место для сохранения еррора
                }

                return View("Index");

            }
            return View(userPost);
        }

        [Authorize]
        public async Task<IActionResult> DeletePost(Guid postGuid)
        {
            CpcmPost? post = null;
            try
            {
                post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            if(post == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            _context.CpcmPosts.Remove(post);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            return StatusCode(200);
        }

        [Authorize]
        public async Task<IActionResult> EditPost(Guid postGuid)
        {
            CpcmPost? post = null;
            try
            {
                post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            if (post == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            UserPostEditModel model = new UserPostEditModel();
            model.Id = post.CpcmPostId;
            model.UserId = post.CpcmUserId;
            model.Text = post.CpcmPostText;
            model.CpcmImages = post.CpcmImages;

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EditPost(UserPostEditModel editPost)
        {
            if(ModelState.IsValid)
            {
                CpcmPost? post = null;
                int i = 0;
                try
                {
                    post = await _context.CpcmPosts.Where(c => c.CpcmPostId == editPost.Id).FirstOrDefaultAsync();
                    i = (await _context.CpcmImages.Where(c => c.CpcmPostId == editPost.Id).OrderBy(k => k.CpcmImageOrder).LastAsync()).CpcmImageOrder;
                }
                catch (DbException)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }
                if(post == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                post.CpcmPostText = editPost.Text;
                post.CpcmPostPublishedDate = DateTime.Now;

                List<CpcmImage>? images = null;
                try
                {
                    images = await _context.CpcmImages.Where(c => !editPost.FilesToDelete.Contains(c.CpcmImageId)).ToListAsync();
                }
                catch(DbException)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                
                if (images != null)
                {
                    _context.CpcmImages.RemoveRange(images);
                    try
                    {
                        await _context.SaveChangesAsync();

                        var imagesAfterDeletion = await _context.CpcmImages.Where(c => c.CpcmPostId == post.CpcmPostId).OrderBy(i => i.CpcmImageOrder).ToListAsync();
                        
                        for (int j = 0; j < imagesAfterDeletion.Count; j++)
                        {
                            imagesAfterDeletion[j].CpcmImageOrder = j;
                        }
                        i = imagesAfterDeletion.Last().CpcmImageOrder;


                        foreach (var image in images)
                        {
                            if (System.IO.File.Exists(image.CpcmImagePath))
                            {
                                System.IO.File.Delete(image.CpcmImagePath);
                            }
                        }

                        

                    }
                    catch (DbException)
                    {
                        return StatusCode(StatusCodes.Status503ServiceUnavailable);
                    }
                }



                List<string> filePaths = new List<string>();
                List<CpcmImage> newImages = new List<CpcmImage>();

                
                foreach (var file in editPost.NewFiles)
                {
                    CheckIFormFile("NewFiles", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (!ModelState.IsValid)
                    {
                        return View(editPost);

                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    filePaths.Add(Path.Combine("wwwroot", "uploads", uniqueFileName));

                    try
                    {
                        using (var fileStream = new FileStream(filePaths.Last(), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        CpcmImage image = new CpcmImage();
                        image.CpcmImageId = Guid.NewGuid();
                        image.CpcmPostId = post.CpcmPostId;
                        image.CpcmImagePath = filePaths.Last();
                        image.CpcmImageOrder = 0;
                        i++;

                        newImages.Add(image);
                    }
                    catch (Exception ex)
                    {
                        Response.StatusCode = 418;
                        ViewData["Message"] = "Не удалось сохранить фотографию на сервере. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                        return View(editPost);
                    }

                }

                _context.CpcmImages.AddRange(newImages);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                    return View(editPost); // TODO Продумать место для сохранения еррора
                }

            }
            return View(editPost);
        }
    }
}
