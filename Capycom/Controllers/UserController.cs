using Capycom.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Packaging;
using NuGet.Protocol.Plugins;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using static NuGet.Packaging.PackagingConstants;
using System.Security.Claims;

namespace Capycom.Controllers
{
    public class UserController : Controller
    {

        private readonly CapycomContext _context;
        private readonly MyConfig _config;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, CapycomContext context, IOptions<MyConfig> config)
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

            CpcmUser? user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(userId)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
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

        //[HttpGet]
		//[Route("User/Index/{id:guid}")]
		//public async Task<ActionResult> Index(UserFilterModel filter)
  //      {
  //          CpcmUser? user;
  //          try
  //          {
  //              user = await _context.CpcmUsers
  //              .Include(c => c.CpcmUserCityNavigation)
  //              .Include(c => c.CpcmUserRoleNavigation)
  //              .Include(c => c.CpcmUserSchoolNavigation)
  //              .Include(c => c.CpcmUserUniversityNavigation)
  //              .Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
  //          }
  //          catch (DbException)
  //          {
  //              Response.StatusCode = 500;
  //              ViewData["ErrorCode"] = 500;
  //              ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
  //              return View("UserError");
  //          }

  //          if (user == null)
  //          {
  //              Response.StatusCode = 404;
  //              ViewData["ErrorCode"] = 404;
  //              ViewData["Message"] = "Пользователь не найден";
  //              return View("UserError");
  //          }

  //          if (user.CpcmUserNickName != null)
  //          {
  //              return RedirectToAction("Index", new { nickName = user.CpcmUserNickName });
  //          }
  //          List<CpcmPost> posts;
  //          try
  //          {
  //              posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == user.CpcmUserId && c.CpcmPostPublishedDate < DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
  //          }
  //          catch (DbException)
  //          {
  //              Response.StatusCode = 500;
  //              ViewData["ErrorCode"] = 500;
  //              ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
  //              return View("UserError");
  //          }
  //          ICollection<PostModel> postsWithLikesCount = new List<PostModel>();
  //          UserProfileAndPostsModel userProfile = new();
  //          userProfile.User = user;
  //          foreach(var postik in posts)
  //          {
  //              postik.User = user;
  //              postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
  //              long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
  //              long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
  //              postsWithLikesCount.Add(new PostModel() { Post = postik, UserOwner=user, LikesCount= likes, RepostsCount= reposts });
  //          }
  //          userProfile.Posts = postsWithLikesCount;
  //          return View(userProfile);
  //      }

        [HttpGet]
        //[Route("User/Index/{nickName}")]
        public async Task<ActionResult> Index(UserFilterModel filter)
        {
   //         if (string.IsNullOrWhiteSpace(filter.NickName))
   //         {
			//	Response.StatusCode = 400;
			//	ViewData["ErrorCode"] = 400;
			//	ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
			//	return View("UserError");
			//}
            CpcmUser? user;
            try
            {
                if (User.Identity.IsAuthenticated && (filter.NickName == null && filter.UserId==null))
                {
                    var guidstring = User.FindFirst(f => f.Type == "CpcmUserId").Value;
					user = await _context.CpcmUsers
							.Where(c => c.CpcmUserId.ToString() == guidstring)
							.Include(c => c.CpcmUserCityNavigation)
							.Include(c => c.CpcmUserRoleNavigation)
							.Include(c => c.CpcmUserSchoolNavigation)
							.Include(c => c.CpcmUserUniversityNavigation)
                            .FirstOrDefaultAsync();
				}
                else if(!User.Identity.IsAuthenticated && (filter.NickName == null && filter.UserId == null))
				{
                    return RedirectToAction("Index", "UserLogIn");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(filter.NickName))
                    {
                        user = await _context.CpcmUsers
                                .Include(c => c.CpcmUserCityNavigation)
                                .Include(c => c.CpcmUserRoleNavigation)
                                .Include(c => c.CpcmUserSchoolNavigation)
                                .Include(c => c.CpcmUserUniversityNavigation)
                                .Where(c => c.CpcmUserNickName == filter.NickName).FirstOrDefaultAsync();
                    } //_context.Entry(user).Reference(u => u.CpcmUserCityNavigation).Load();
                    else
                    {
                        user = await _context.CpcmUsers
                                .Include(c => c.CpcmUserCityNavigation)
                                .Include(c => c.CpcmUserRoleNavigation)
                                .Include(c => c.CpcmUserSchoolNavigation)
                                .Include(c => c.CpcmUserUniversityNavigation)
                                .Where(c => c.CpcmUserId == filter.UserId).FirstOrDefaultAsync();
                    } 
                }
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null||user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            List<CpcmPost> posts;
            try
            {
                var date = DateTime.Now;

				posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == user.CpcmUserId && c.CpcmPostPublishedDate < date).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).AsNoTracking().ToListAsync();
                if (HttpContext.User.Identity.IsAuthenticated && user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
                {
                    var friend = await _context.CpcmUserfriends.Where(f => f.CmcpUserId == user.CpcmUserId && f.CmcpFriendId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync();
                    if (friend == null)
                    {
                        await _context.CpcmUserfriends.Where(f => f.CmcpUserId.ToString() == User.FindFirstValue("CpcmUserId") && f.CmcpFriendId == user.CpcmUserId).FirstOrDefaultAsync();
                    }
                    if (friend != null)
                    {
                        user.IsFriend = friend.CpcmFriendRequestStatus;
					}
					var follower = await _context.CpcmUserfollowers.Where(f => f.CpcmFollowerId.ToString() == User.FindFirstValue("CpcmUserId") && f.CpcmUserId == user.CpcmUserId).FirstOrDefaultAsync();
					if (follower == null)
                    {
						user.IsFollowing = false;
					}
                    else
                    {
                        user.IsFollowing = true;
                    }
                }
                else
                {
                    user.IsFriend = null;
                    user.IsFollowing = false;
                }
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            ICollection<PostModel> postsWithLikesCount = new List<PostModel>();
            UserProfileAndPostsModel userProfile = new();
            userProfile.User = user;
            foreach (var postik in posts)
            {
				postik.User = user;
				postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
                long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();

                if(User.Identity.IsAuthenticated)
                {
					long liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId} AND CPCM_UserID = {User.FindFirstValue("CpcmUserId")}").CountAsync();
                    if(liked >0)
					    postik.IsLiked =true;
                    else
                        postik.IsLiked =false;
                }
                postsWithLikesCount.Add(new PostModel() { Post = postik, UserOwner = user, LikesCount = likes, RepostsCount = reposts });
            }
            userProfile.Posts = postsWithLikesCount;
            return View(userProfile);
        }




        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Edit(string id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }

            CpcmUser? user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Пользователь был заблокирован";
                return View("UserError");
            }
            if (user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }

            ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
            ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            UserEditAboutDataModel userModel = new();
            userModel.CpcmUserId = user.CpcmUserId;
            userModel.CpcmUserAbout = user.CpcmUserAbout;
            userModel.CpcmUserCity = user.CpcmUserCity;
            userModel.CpcmUserSite = user.CpcmUserSite;
            userModel.CpcmUserBooks = user.CpcmUserBooks;
            userModel.CpcmUserFilms = user.CpcmUserFilms;
            userModel.CpcmUserMusics = user.CpcmUserMusics;
            userModel.CpcmUserSchool = user.CpcmUserSchool;
            userModel.CpcmUserUniversity = user.CpcmUserUniversity;
            userModel.CpcmUserFirstName = user.CpcmUserFirstName;
            userModel.CpcmUserSecondName = user.CpcmUserSecondName;
            userModel.CpcmUserAdditionalName = user.CpcmUserAdditionalName;

            return View(userModel);

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserEditAboutDataModel user)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", user.CpcmUserId))
            {
                Response.StatusCode = 403;
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
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                }

                if (cpcmUser == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                if (cpcmUser.CpcmUserBanned)
                {
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пользователь был заблокирован";
                    return View("UserError");
                }
                if (cpcmUser.CpcmIsDeleted)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                cpcmUser.CpcmUserAbout = user.CpcmUserAbout?.Trim();
                cpcmUser.CpcmUserCity = user.CpcmUserCity;
                cpcmUser.CpcmUserSite = user.CpcmUserSite?.Trim();
                cpcmUser.CpcmUserBooks = user.CpcmUserBooks?.Trim();
                cpcmUser.CpcmUserFilms = user.CpcmUserFilms?.Trim();
                cpcmUser.CpcmUserMusics = user.CpcmUserMusics?.Trim();
                cpcmUser.CpcmUserSchool = user.CpcmUserSchool;
                cpcmUser.CpcmUserUniversity = user.CpcmUserUniversity;


                cpcmUser.CpcmUserFirstName = user.CpcmUserFirstName.Trim();
                cpcmUser.CpcmUserSecondName = user.CpcmUserSecondName.Trim();
                cpcmUser.CpcmUserAdditionalName = user.CpcmUserAdditionalName?.Trim();


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
                            cpcmUser.CpcmUserImagePath = filePathUserImage.Replace("wwwroot", "");
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
                            cpcmUser.CpcmUserCoverPath = filePathUserCoverImage.Replace("wwwroot", "");
                        }
                        catch (Exception ex)
                        {
                            cpcmUser.CpcmUserCoverPath = null;
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
                    ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
                    ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
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
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
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

            ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
            ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            return View(user);
        }




        [Authorize]
        [HttpPost]
        public async Task<ActionResult> EditIdentity(string id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }
            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(id)).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }
            UserEditIdentityModel userModel = new();
            userModel.CpcmUserEmail = user.CpcmUserEmail;
            userModel.CpcmUserNickName = user.CpcmUserNickName;
            userModel.CpcmUserTelNum = user.CpcmUserTelNum;
            userModel.CpcmUserId = user.CpcmUserId;
            return View(userModel);

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
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                }

                if (cpcmUser == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                if (cpcmUser.CpcmUserBanned)
                {
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пользователь был заблокирован";
                    return View("UserError");
                }
                if (cpcmUser.CpcmIsDeleted)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                cpcmUser.CpcmUserEmail = user.CpcmUserEmail.Trim();
                cpcmUser.CpcmUserTelNum = user.CpcmUserTelNum.Trim();

                if (user.CpcmUserNickName == null || user.CpcmUserNickName == "")
                {
                    cpcmUser.CpcmUserNickName = null;
                }
                else
                {
                    cpcmUser.CpcmUserNickName = user.CpcmUserNickName?.Trim();
                }


                if (!string.IsNullOrEmpty(user.CpcmUserPwd))
                {
                    cpcmUser.CpcmUserPwdHash = MyConfig.GetSha256Hash(user.CpcmUserPwd, cpcmUser.CpcmUserSalt, _config.ServerSol);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                }


                return RedirectToAction($"Index\\{user.CpcmUserId}");
            }

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BanUnbanUser(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
                return StatusCode(403);
            }
            try
            {
                var user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id && c.CpcmIsDeleted==false).FirstOrDefaultAsync();
                if (user == null)
                {
                    return StatusCode(404);
                }
                user.CpcmUserBanned = !user.CpcmUserBanned;
                await _context.SaveChangesAsync();
                return StatusCode(200 , new {status=true});
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
        }




        public async Task<ActionResult> Friends(UserFilterModel filters)
        {
            CpcmUser user = null ;
            try
            {
                if (filters.NickName == null)
                {
                    user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync(); 
                }
                else
                {
                    user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == filters.NickName).FirstOrDefaultAsync();
				}
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }

			//_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
			IQueryable<CpcmUser> friendList1;
			IQueryable<CpcmUser> friendList2;
            try
            {
                friendList1 =  _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpFriend);
                friendList2 =  _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpUser);
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            friendList1.Concat(friendList2);

			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				friendList1 = friendList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				friendList1 = friendList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				friendList1 = friendList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			return View(result);


        }
        [HttpPost]
        public async Task<ActionResult> GetNextFriends(UserFilterModel filters)
        {
            CpcmUser user = null;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync(); 
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                return StatusCode(500);
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                return StatusCode(404);
            }




			//_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
			IQueryable<CpcmUser> friendList1;
			IQueryable<CpcmUser> friendList2;
			try
            {
                friendList1 =  _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus == true && c.CmcpFriendId.CompareTo(filters.lastId) > 0).Select(c => c.CmcpFriend);
                friendList2 =  _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true && c.CmcpUserId.CompareTo(filters.lastId) > 0).Select(c => c.CmcpUser);
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                return StatusCode(500);
            }

            friendList1.Concat(friendList2);
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				friendList1 = friendList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				friendList1 = friendList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				friendList1 = friendList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}
			var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			return PartialView(result);
        }

        ////[Route("User/Friends/{nickName}")]
        //public async Task<ActionResult> FriendsByNick(string nickName)
        //{
        //    CpcmUser user;
        //    try
        //    {
        //        user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstOrDefaultAsync();
        //    }
        //    catch (DbException)
        //    {
        //        Response.StatusCode = 500;
        //        ViewData["ErrorCode"] = 500;
        //        ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
        //        return View("UserError");
        //    }

        //    if (user == null || user.CpcmIsDeleted)
        //    {
        //        Response.StatusCode = 404;
        //        ViewData["ErrorCode"] = 404;
        //        ViewData["Message"] = "Пользователь не найден";
        //        return View("UserError");
        //    }

        //    //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
        //    List<CpcmUser> friendList1;
        //    List<CpcmUser> friendList2;
        //    try
        //    {
        //        friendList1 = await _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpFriend).OrderBy(u => u.CpcmUserId).Take(5).ToListAsync();
        //        friendList2 = await _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true).Select(c => c.CmcpUser).OrderBy(u => u.CpcmUserId).Take(10 - friendList1.Count).ToListAsync();
        //    }
        //    catch (DbException)
        //    {
        //        Response.StatusCode = 500;
        //        ViewData["ErrorCode"] = 500;
        //        ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
        //        return View("UserError");
        //    }

        //    friendList1.AddRange(friendList2);

        //    return View(friendList1);
        //}
        //[HttpPost]
        ////[Route("User/GetNextFriends/{nickName}")]
        //public async Task<ActionResult> GetNextFriendsByNick(string nickName, Guid lastId)
        //{
        //    CpcmUser user;
        //    try
        //    {
        //        user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == nickName).FirstOrDefaultAsync();
        //    }
        //    catch (DbException)
        //    {
        //        Response.StatusCode = 500;
        //        return StatusCode(500);
        //    }

        //    if (user == null || user.CpcmIsDeleted)
        //    {
        //        Response.StatusCode = 404;
        //        return StatusCode(404);
        //    }

        //    //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
        //    List<CpcmUser> friendList1;
        //    List<CpcmUser> friendList2;
        //    try
        //    {
        //        friendList1 = await _context.CpcmUserfriends.Where(c => c.CmcpUserId == user.CpcmUserId && c.CpcmFriendRequestStatus == true && c.CmcpFriendId.CompareTo(lastId) > 0).Select(c => c.CmcpFriend).OrderBy(u => u.CpcmUserId).Take(5).ToListAsync();
        //        friendList2 = await _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && c.CpcmFriendRequestStatus == true && c.CmcpUserId.CompareTo(lastId) > 0).Select(c => c.CmcpUser).OrderBy(u => u.CpcmUserId).Take(10 - friendList1.Count).ToListAsync();
        //    }
        //    catch (DbException)
        //    {
        //        Response.StatusCode = 500;
        //        return StatusCode(500);
        //    }

        //    friendList1.AddRange(friendList2);

        //    return Json(friendList1);
        //}




        public async Task<ActionResult> Followers(UserFilterModel filters)
        {
            CpcmUser user;
            try
            {
                if (filters.NickName==null)
                {
                    user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync(); 
                }
                else
                {
					user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == filters.NickName).FirstOrDefaultAsync();
				}
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            IQueryable<CpcmUser> followerList1;
            //List<CpcmUser> followerList2;
            try
            {
                followerList1 = _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmFollower);
                //followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

			//followerList1.AddRange(followerList2);
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				followerList1 = followerList1.Where(u => u.CpcmUserCity == filters.CityId);
				
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				followerList1 = followerList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				followerList1 = followerList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();

			return View(followerList1);
        }
		[HttpPost]
		public async Task<ActionResult> GetNextFollowers(UserFilterModel filters)
        {
			CpcmUser user;
			try
            {
				if (filters.NickName == null)
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync();
				}
				else
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == filters.NickName).FirstOrDefaultAsync();
				}
			}
            catch (Exception)
            {
                Response.StatusCode = 500;
                return StatusCode(500);
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                return StatusCode(404);
            }

            //_context.CpcmUserfriends.Select(c => c.CmcpFriend).Where(c => c.CmcpUserId == id).Include(c => c.CmcpFriend).ToList();
            IQueryable<CpcmUser> followerList1;
            //List<CpcmUser> followerList2;
            try
            {
                followerList1 =  _context.CpcmUserfollowers.Where(c => c.CpcmUserId == user.CpcmUserId && c.CpcmFollowersId.CompareTo(filters.lastId) > 0).Select(c => c.CpcmFollower);
                //followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return StatusCode(500);
            }
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				followerList1 = followerList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				followerList1 = followerList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				followerList1 = followerList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				followerList1 = followerList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			//followerList1.AddRange(followerList2);

			return PartialView(followerList1);
        }

		public async Task<IActionResult> Groups(GroupFilterModel filters)
		{
			CpcmUser user;
			try
			{
				if (filters.NickName == null)
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync();
				}
				else
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == filters.NickName).FirstOrDefaultAsync();
				}
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (user == null || user.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Пользователь не найден";
				return View("UserError");
			}


			IQueryable<CpcmGroup> groupsList;
			//List<CpcmUser> followerList2;
			try
			{
				groupsList = _context.CpcmGroupfollowers.Where(c => c.CpcmUserId == user.CpcmUserId).Select(c => c.CpcmGroup);
				//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				groupsList = groupsList.Where(u => u.CpcmGroupCity == filters.CityId);

			}
			if (!string.IsNullOrEmpty(filters.Name))
			{
				//ViewData["scgoolId"]=schoolId;
				//groupsList = groupsList.Where(u => u.CpcmGroupName == filters.Name);
				groupsList = groupsList.Where(u => EF.Functions.Like(u.CpcmGroupName, $"%{filters.Name}%"));
			}
			if (filters.SubjectID.HasValue)
			{
				//ViewData["universityId"] = universityId;
				groupsList = groupsList.Where(u => u.CpcmGroupSubject == filters.SubjectID);
			}


			var result = await groupsList.OrderBy(p => p.CpcmGroupId).Take(10).ToListAsync();
			return View(groupsList);
		}

		public async Task<IActionResult> GetNextGroups(GroupFilterModel filters)
		{
			CpcmUser user;
			try
			{
				if (filters.NickName == null)
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync();
				}
				else
				{
					user = await _context.CpcmUsers.Where(c => c.CpcmUserNickName == filters.NickName).FirstOrDefaultAsync();
				}
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (user == null || user.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Пользователь не найден";
				return View("UserError");
			}


			IQueryable<CpcmGroup> groupsList;
			//List<CpcmUser> followerList2;
			try
			{
				groupsList = _context.CpcmGroupfollowers.Where(c => c.CpcmUserId == user.CpcmUserId && c.CpcmGroupId.CompareTo(filters.lastId) > 1).Select(c => c.CpcmGroup);
				//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				groupsList = groupsList.Where(u => u.CpcmGroupCity == filters.CityId);

			}
			if (!string.IsNullOrEmpty(filters.Name))
			{
				//ViewData["scgoolId"]=schoolId;
				//groupsList = groupsList.Where(u => u.CpcmGroupName == filters.Name);
				groupsList = groupsList.Where(u => EF.Functions.Like(u.CpcmGroupName, $"%{filters.Name}%"));
			}
			if (filters.SubjectID.HasValue)
			{
				//ViewData["universityId"] = universityId;
				groupsList = groupsList.Where(u => u.CpcmGroupSubject == filters.SubjectID);
			}


			var result = await groupsList.OrderBy(p => p.CpcmGroupId).Take(10).ToListAsync();
			return PartialView(groupsList);
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
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Пользователь заблокирован и не может быть удалён";
                return View("UserError");
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
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Пользователь заблокирован и не может быть удалён";
                return View("UserError");
            }
            //_context.CpcmUsers.Remove(user);
            user.CpcmIsDeleted = true;

            try
            {
                await _context.SaveChangesAsync();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            
            return RedirectToAction("UserLogIn", "Index");

        }

		[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(Guid CpcmUserId)
        {

            CpcmUserfollower follower = new();
            follower.CpcmFollowersId = Guid.NewGuid();
            follower.CpcmUserId = CpcmUserId;
            follower.CpcmFollowerId = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            
			

			try
            {
                Guid id = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);

				var follow = await _context.CpcmUserfollowers.Where(e => e.CpcmUserId == CpcmUserId && e.CpcmFollowerId == id).FirstOrDefaultAsync();
                if (follow == null)
                {
					_context.CpcmUserfollowers.Add(follower);
					await _context.SaveChangesAsync();
				}                    
                else
                    return StatusCode(200, new { status = false, message = "Вы уже подписаны на этого человека" });
            }
            catch (DbException)
            {
                return StatusCode(500);
            }

            return StatusCode(200, new {status=true});
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
                return StatusCode(500);
            }


            if (follow == null)
            {
                return StatusCode(404);
            }

            _context.Remove(follow);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }

            return StatusCode(200, new {status=true});

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
                return StatusCode(500);
            }

            return StatusCode(200, new { status = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerToFriendRequests(Guid CpcmUserId, bool status)
        {
            CpcmUserfriend? friendRequest;
            try
            {
                friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value
                    && c.CmcpFriendId == CpcmUserId).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            if (friendRequest == null)
            {
                return StatusCode(404);
            }

            friendRequest.CpcmFriendRequestStatus = status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }

            return StatusCode(200, new { status = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFriendRequests(Guid CpcmUserId)
        {
            CpcmUserfriend? friendRequest;
            try
            {
                friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId.ToString() == HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value
                   && c.CmcpFriendId == CpcmUserId).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            if (friendRequest == null)
            {
                return StatusCode(404);
            }
            _context.CpcmUserfriends.Remove(friendRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }

            return StatusCode(200, new { status = true });
        }
		[Authorize]
		public async Task<IActionResult> ViewFriendRequests(UserFilterModel filters)
        {
			CpcmUser user = null;
			try
			{
				user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (user == null || user.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				return StatusCode(404);
			}
            if (user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
            {
                Response.StatusCode = 403;

                return StatusCode(403);
            }
			IQueryable<CpcmUser> friendList1;
			try
			{
				friendList1 = _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && (c.CpcmFriendRequestStatus == null|| c.CpcmFriendRequestStatus == false)).Select(c => c.CmcpUser);//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				friendList1 = friendList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				friendList1 = friendList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				friendList1 = friendList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			//followerList1.AddRange(followerList2);

			return View(friendList1);
		}

		public async Task<IActionResult> GetNextFriendRequests(UserFilterModel filters)
		{
			CpcmUser user = null;
			try
			{
				user = await _context.CpcmUsers.Where(c => c.CpcmUserId == filters.UserId).FirstOrDefaultAsync();
			}
			catch (DbException)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (user == null || user.CpcmIsDeleted)
			{
				Response.StatusCode = 404;
				return StatusCode(404);
			}
			if (user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
			{
				Response.StatusCode = 403;

				return StatusCode(403);
			}
			IQueryable<CpcmUser> friendList1;
			try
			{
				friendList1 = _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && (c.CpcmFriendRequestStatus == null || c.CpcmFriendRequestStatus == false) && c.CmcpUserId.CompareTo(filters.lastId) > 0).Select(c => c.CmcpUser);//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
			catch (Exception)
			{
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				friendList1 = friendList1.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				friendList1 = friendList1.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				friendList1 = friendList1.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				friendList1 = friendList1.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
			//followerList1.AddRange(followerList2);

			return PartialView(friendList1);
		}
		[Authorize]
        public async Task<IActionResult> CreatePost()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePostP(UserPostModel userPost)
        {
            //if (userPost.Text == null && userPost.Files.Count > 0)
            //{
            //    return View(userPost);
            //}

            if (ModelState.IsValid)
            {
                if(string.IsNullOrEmpty(userPost.Text)||string.IsNullOrWhiteSpace(userPost.Text) && userPost.Files == null)
                {
					Response.StatusCode = 400;
					ViewData["ErrorCode"] = 400;
					ViewData["Message"] = "Нельзя создавать пустой пост";
					return View(userPost);
				}
                CpcmPost post = new CpcmPost();

                post.CpcmPostText = userPost.Text.Trim();
                post.CpcmPostId = Guid.NewGuid();
                post.CpcmPostFather = userPost.PostFatherId;
                post.CpcmPostCreationDate = DateTime.UtcNow;
                if (userPost.Published == null)
                {
					post.CpcmPostPublishedDate = post.CpcmPostCreationDate;
				}
                else
                {
					post.CpcmPostPublishedDate = userPost.Published;
				}
                
                post.CpcmUserId = Guid.Parse(User.FindFirst(c => c.Type == "CpcmUserId").Value);
 

                List<string> filePaths = new List<string>();
                List<CpcmImage> images = new List<CpcmImage>();

                if (userPost.PostFatherId != null)
                {
                    int i = 0;
                    if (userPost.Files!=null)
                    {
                        foreach (IFormFile file in userPost.Files)
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
                                image.CpcmImagePath = filePaths.Last().Replace("wwwroot", "");
                                image.CpcmImageOrder = 0;
                                i++;

                                images.Add(image);
                            }
                            catch (Exception ex)
                            {
                                Response.StatusCode = 500;
                                ViewData["ErrorCode"] = 500;
                                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                                return View(userPost);
                            }

                        }
                        post.CpcmImages = images; 
                    }
                    _context.CpcmImages.AddRange(images); 
                }
                _context.CpcmPosts.Add(post);
                try
                {
                    if (userPost.PostFatherId !=null)
                    {
                        var fatherPost = await _context.CpcmPosts.Where(p => p.CpcmPostId == userPost.PostFatherId).FirstOrDefaultAsync(); 
                        if(fatherPost==null || fatherPost.CpcmPostPublishedDate > DateTime.Now)
                        {
                            return StatusCode(200, new {status=false,message= "Нельзя репостить неопубликованный пост" });
                        }
                        if (fatherPost.CpcmPostBanned)
                        {
                            return StatusCode(200, new { status = false, message = "Нельзя репостить этот пост" });
                        }
                        if (fatherPost.CpcmIsDeleted)
                        {
                            return StatusCode(404, new { message = "Не найден родительский пост" });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
                {
                    foreach (var item in filePaths)
                    {
                        if (System.IO.File.Exists(item))
                        {
                            System.IO.File.Delete(item);
                        }
                    }
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View(userPost); // TODO Продумать место для сохранения еррора
                }

                if (userPost.PostFatherId == null)
                {
                    return RedirectToAction("Index"); 
                }
                else
                {
                    return StatusCode(200, new { status = true });
                }

            }
            if (userPost.PostFatherId == null)
            {
                return View(userPost); 
            }
            else
            {
                return StatusCode(200, new {status=false, message="Репост имел неккоректный вид. Возможно вы попытались прикрепить файлы. Однако этого нельзя делать для репостов.", errors= ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList() });
            }
        }




        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Guid postGuid)
        {


            CpcmPost? post = null;
            try
            {
                post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            if (post == null || !post.CpcmIsDeleted)
            {
                return StatusCode(404);
            }
            if (post.CpcmPostBanned)
            {
                return StatusCode(403);
            }

            if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()))
            {
                return StatusCode(403);
            }


            post.CpcmIsDeleted = true;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            return StatusCode(200, new { status = true });
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
                return StatusCode(500);
            }

            if (post == null)
            {
                return StatusCode(404);
            }
            if (post.CpcmIsDeleted)
            {
                return StatusCode(404);
            }
            if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()) || post.CpcmPostBanned == true)
            {
                return View("Index");
            }
            
            
            UserPostEditModel model = new UserPostEditModel();
            model.Id = post.CpcmPostId;
            model.UserId = post.CpcmUserId;
            model.Text = post.CpcmPostText;
            model.CpcmImages = post.CpcmImages;
            model.NewPublishDate = post.CpcmPostPublishedDate;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(UserPostEditModel editPost)
        {
            //if (editPost.Text == null && editPost.FilesToDelete.Count == 0 && editPost.NewFiles.Count == 0)
            //{
            //    return View(editPost);
            //}


            if (ModelState.IsValid)
            {
                CpcmPost? post = null;
                try
                {
                    post = await _context.CpcmPosts.Include(c => c.CpcmImages).Where(c => c.CpcmPostId == editPost.Id).FirstOrDefaultAsync();

                }
                catch (DbException)
                {
                    return StatusCode(500);
                }
                if (post == null)
                {
                    return StatusCode(404);
                }
                if (post.CpcmIsDeleted)
                {
                    return StatusCode(404);
                }
                if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()) || post.CpcmPostBanned == true)
                {
                    return StatusCode(403);
                }
                if (post.CpcmImages.Count - editPost.FilesToDelete.Count + editPost.NewFiles.Count > 4)
                {
                    ModelState.AddModelError("NewFiles", "В посте не может быть больше 4 фотографий");
                    return View(editPost);
                }

                post.CpcmPostText = editPost.Text.Trim();

				if (editPost.NewPublishDate == null)
				{
					post.CpcmPostPublishedDate = DateTime.UtcNow;
				}
				else
				{
					post.CpcmPostPublishedDate = editPost.NewPublishDate;
				}

				//post.CpcmPostPublishedDate = DateTime.UtcNow;
                if(post.CpcmPostFather != null)
                {
                    editPost.FilesToDelete = new List<Guid>();
                    editPost.NewFiles = new();
                }

                var imageLast = post.CpcmImages.Where(c => c.CpcmPostId == editPost.Id).OrderBy(k => k.CpcmImageOrder).LastOrDefault();
                int i = 0;
                if (imageLast != null)
                {
                    i = imageLast.CpcmImageOrder;
                }
                List<string> filePaths = new List<string>();
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
                        image.CpcmImagePath = filePaths.Last().Replace("wwwroot", "");
                        image.CpcmImageOrder = 0;
                        i++;

                        post.CpcmImages.Add(image);
                    }
                    catch (Exception ex)
                    {
                        foreach (var uploadedfile in filePaths)
                        {
                            if (System.IO.File.Exists(uploadedfile)){
                                System.IO.File.Delete(uploadedfile);
                            }
                        }
                        Response.StatusCode = 500;
                        ViewData["ErrorCode"] = 500;
                        ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                        return View(editPost);
                    }

                }


                List<CpcmImage>? images = post.CpcmImages.Where(c => !editPost.FilesToDelete.Contains(c.CpcmImageId)).ToList(); //TODO возможно ! тут не нужен 
                if (images!=null||images.Count != 0)
                {
                    //_context.CpcmImages.RemoveRange(images);
                    foreach (var item in images)
                    {
                        post.CpcmImages.Remove(item);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        var imagesAfterDeletion = post.CpcmImages.Where(c => c.CpcmPostId == post.CpcmPostId).OrderBy(i => i.CpcmImageOrder).ToList();

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

                        foreach (var path in filePaths)
                        {
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                        }

                        return StatusCode(500);
                    }
                }


                try
                {
                    if(post.CpcmPostText == null && (await _context.CpcmImages.Where(p => p.CpcmPostId == post.CpcmPostId).ToListAsync()).Count == 0)
                    {
                        ModelState.AddModelError("Text", "Нельзя создать пустой пост");
                        return View(editPost);
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbException)
                {
                    Response.StatusCode = 418;
                    ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                    return View(editPost); // TODO Продумать место для сохранения еррора
                }
                RedirectToAction("Index");

            }
            return View(editPost);
        }

        [HttpPost]
        public async Task<IActionResult> GetNextPosts(Guid userId, Guid lastPostId)
        {
            List<CpcmPost> posts;
            List<PostModel> postModels = new List<PostModel>();
            try
            {
                var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == lastPostId).FirstOrDefaultAsync();
                if (post == null)
                {
                    return StatusCode(404);
                }
				var date = DateTime.Now;
				posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == userId).Where(c => c.CpcmPostPublishedDate < post.CpcmPostPublishedDate && c.CpcmPostPublishedDate < date).AsNoTracking().Take(10).ToListAsync();
                foreach (var postik in posts)
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                    long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
                    long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
                    postModels.Add(new() { Post = postik, LikesCount = likes, RepostsCount = reposts });
                }
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            return PartialView(postModels);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetNextNotPublishedPosts(Guid userId, Guid lastPostId)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", userId))
            {
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }
            List<CpcmPost> posts;
            List<PostModel> postModels = new List<PostModel>();
            try
            {
                var lastPost = await _context.CpcmPosts.Where(c => c.CpcmPostId == lastPostId).FirstOrDefaultAsync();
                if (lastPost == null)
                {
                    return StatusCode(404);
                }
				var date = DateTime.Now;
				posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == userId && c.CpcmPostId == lastPostId).Where(c => c.CpcmPostPublishedDate < lastPost.CpcmPostPublishedDate && c.CpcmPostPublishedDate > date).AsNoTracking().Take(10).ToListAsync();
                foreach (var postik in posts)
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                    //long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = '{postik.CpcmGroupId}'");
                    //long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = '{postik.CpcmGroupId}'");
                    postModels.Add(new() { Post = postik, LikesCount = 0, RepostsCount = 0 });
                }
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
            return PartialView(postModels);
        }
		[Authorize]
		[HttpGet]
        public async Task<IActionResult> NotPublishedPosts(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }

            List<CpcmPost> posts;
            List<PostModel> postModels = new List<PostModel>();
            try
            {
				var date = DateTime.Now;
				posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == id && c.CpcmPostPublishedDate > date).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
                foreach (var postik in posts)
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                    postModels.Add(new() { Post = postik, LikesCount = 0, RepostsCount = 0 });
                }
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            return View(postModels);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BanUnbanPost(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True"))
            {
                return StatusCode(403);
            }
            try
            {
				var date = DateTime.Now;
				var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == id && c.CpcmPostPublishedDate < date).FirstOrDefaultAsync();
                if(post == null || post.CpcmIsDeleted==true)
                {
                    return StatusCode(404);
                }
                post.CpcmPostBanned = !post.CpcmPostBanned;
                await _context.SaveChangesAsync();
                return StatusCode(200, new {status=true});
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
        }


        //public async Task<IActionResult> FindUser()
        //{
        //    return View();
        //}

        [HttpGet]
        public async Task<IActionResult> FindUser(UserFilterModel filters)
        {
            var query = _context.CpcmUsers.AsQueryable();
            if (filters.UserId.HasValue)
            {
                //ViewData["id"] = id;
                query = query.Where(u => u.CpcmUserId == filters.UserId);
            }
            if (!string.IsNullOrEmpty(filters.NickName))
            {
                //ViewData["nick"]=nick;
                query = query.Where( u => EF.Functions.Like(u.CpcmUserNickName, $"%{filters.NickName}%"));
			}
            if (filters.CityId.HasValue)
            {
                //ViewData["cityId"]=cityId;
                query = query.Where(u => u.CpcmUserCity == filters.CityId);
            }
            if (filters.SchoolId.HasValue)
            {
                //ViewData["scgoolId"]=schoolId;
                query = query.Where(u => u.CpcmUserSchool == filters.SchoolId);
            }
            if (filters.UniversityId.HasValue)
            {
                //ViewData["universityId"] = universityId;
                query = query.Where(u => u.CpcmUserUniversity == filters.UniversityId);
            }
            if (!string.IsNullOrEmpty(filters.FirstName))
            {
                //ViewData["firstName"] = firstName;
                query = query.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
            if (!string.IsNullOrEmpty(filters.SecondName))
            {
                //ViewData["secondName"] = secondName;
                query = query.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
            }
            if (!string.IsNullOrEmpty(filters.AdditionalName))
            {
                //ViewData["additionalName"] = additionalName;
                query = query.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
            }

            try
            {
                var rez = await query.OrderBy(u => u.CpcmUserId).Take(10).ToListAsync();
                return View(rez);
            }
            catch (DbException)
            {
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
                //return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindNextUser(UserFilterModel filters)
        {
            var query = _context.CpcmUsers.AsQueryable();
			if (filters.UserId.HasValue)
			{
				//ViewData["id"] = id;
				query = query.Where(u => u.CpcmUserId == filters.UserId);
			}
			if (!string.IsNullOrEmpty(filters.NickName))
			{
				//ViewData["nick"]=nick;
				query = query.Where(u => EF.Functions.Like(u.CpcmUserNickName, $"%{filters.NickName}%"));
			}
			if (filters.CityId.HasValue)
			{
				//ViewData["cityId"]=cityId;
				query = query.Where(u => u.CpcmUserCity == filters.CityId);
			}
			if (filters.SchoolId.HasValue)
			{
				//ViewData["scgoolId"]=schoolId;
				query = query.Where(u => u.CpcmUserSchool == filters.SchoolId);
			}
			if (filters.UniversityId.HasValue)
			{
				//ViewData["universityId"] = universityId;
				query = query.Where(u => u.CpcmUserUniversity == filters.UniversityId);
			}
			if (!string.IsNullOrEmpty(filters.FirstName))
			{
				//ViewData["firstName"] = firstName;
				query = query.Where(u => EF.Functions.Like(u.CpcmUserFirstName, $"%{filters.FirstName}%"));
			}
			if (!string.IsNullOrEmpty(filters.SecondName))
			{
				//ViewData["secondName"] = secondName;
				query = query.Where(u => EF.Functions.Like(u.CpcmUserSecondName, $"%{filters.SecondName}%"));
			}
			if (!string.IsNullOrEmpty(filters.AdditionalName))
			{
				//ViewData["additionalName"] = additionalName;
				query = query.Where(u => EF.Functions.Like(u.CpcmUserAdditionalName, $"%{filters.AdditionalName}%"));
			}

			try
            {
                var rez = await query.Where(u => u.CpcmUserId.CompareTo(filters.lastId) > 0).OrderBy(u => u.CpcmUserId).Take(10).ToListAsync();
                return Json(rez);
            }
            catch (DbException)
            {
                return StatusCode(500);
            }
        }

        private bool CheckUserPrivilege(string claimType, string claimValue, string id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id || c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }
        private bool CheckUserPrivilege(string claimType, string claimValue, Guid id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id.ToString() || c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }
        private bool CheckUserPrivilege(string claimType, string claimValue)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
                return false;
            }
            return true;
        }

        private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
        {
            var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
            if (father != null)
            {
                father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
                father.User = await _context.CpcmUsers.Where(p => p.CpcmUserId==father.CpcmUserId).FirstOrDefaultAsync();
                father.Group = await _context.CpcmGroups.Where(p => p.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
			}
            return father;
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
            if (string.IsNullOrWhiteSpace(CpcmUserEmail))
            {
                return Json(data: "Email не может быть пустым или состоять из одних пробелов");
            }

            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmCanEditUsers" && c.Value == "True");
            CpcmUserEmail = CpcmUserEmail.Trim();
            if (CpcmUserEmail.Contains("admin") || CpcmUserEmail.Contains("webmaster") || CpcmUserEmail.Contains("abuse") && authFactor == null)
            {
                return Json(data: $"{CpcmUserEmail} зарезервировано");
            }

            bool rez = false;
            try
            {
                rez = !await _context.CpcmUsers.AnyAsync(e => e.CpcmUserEmail == CpcmUserEmail && e.CpcmUserId != CpcmUserId);
            }
            catch (DbException)
            {
				//StatusCode(500);
				return Json(data: "Не удалось установить соединение с сервером");
			}
			if (!rez)
				return Json(data: "Данный Email уже занят");
			return Json(rez);
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckNickName(string CpcmUserNickName, Guid CpcmUserId)
        {
            if (CpcmUserNickName == null || CpcmUserNickName.All(char.IsWhiteSpace) || CpcmUserNickName == string.Empty)
            {
                return Json(true);
            }
            CpcmUserNickName = CpcmUserNickName.Trim();

            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmCanEditUsers" && c.Value == "True");
            if (CpcmUserNickName.Contains("admin") || CpcmUserNickName.Contains("webmaster") || CpcmUserNickName.Contains("abuse") && authFactor==null)
            {
                return Json(data: $"{CpcmUserNickName} зарезервировано");
            }
            bool rez = false;
            try
            {
                rez = !await _context.CpcmUsers.AnyAsync(e => e.CpcmUserNickName == CpcmUserNickName && e.CpcmUserId != CpcmUserId);
            }
            catch (DbException)
            {
				//StatusCode(500);
				return Json(data: "Не удалось установить соединение с сервером");
			}
			if (!rez)
				return Json(data: "Данный nickname уже занят");
			return Json(rez);
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckPhone(string CpcmUserTelNum, Guid CpcmUserId)
        {
            if (string.IsNullOrWhiteSpace(CpcmUserTelNum))
            {
                return Json(data: "Телефон не может быть пустым или состоять из одних пробелов");
            }
            CpcmUserTelNum = CpcmUserTelNum.Trim();
            bool rez = false;
            try
            {
                rez = !await _context.CpcmUsers.AnyAsync(e => e.CpcmUserTelNum == CpcmUserTelNum && e.CpcmUserId != CpcmUserId);
            }
            catch (DbException)
            {
				//StatusCode(500);
				return Json(data: "Не удалось установить соединение с сервером");
			}
			if (!rez)
				return Json(data: "Данный телефон уже занят");
			return Json(rez);
        }
        [HttpPost]//TODO: Объединить с методами при регистрации
        public async Task<IActionResult> CheckPwd(string pwd)
        {
            if(pwd == null || pwd == "")
            {
                return Json(true);
            }
            else
            {
                bool rez = false;
                try
                {
                    rez = Regex.Match(pwd, @"(^$)|^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$").Success;
                }
                catch (DbException)
                {
                    //StatusCode(500);
                    return Json(false);
                }
               return Json(rez);
            }
        }


    }
}
