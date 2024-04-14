using Capycom.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Capycom.Enums;
using Serilog;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols.Configuration;
using static NuGet.Packaging.PackagingConstants;

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

            CpcmUser? user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(userId)).FirstOrDefaultAsync();
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null)
            {
                Log.Warning("Пользователь не найден");
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь удалён");
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


        [HttpGet]
        public async Task<ActionResult> Index(UserFilterModel filter)
        {
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
                    Log.Information("Пользователь авторизован {u} и просматривает свою страницу", user.CpcmUserNickName);
				}
                else if(!User.Identity.IsAuthenticated && (filter.NickName == null && filter.UserId == null))
				{
                    Log.Information("Пользователь не авторизован и пытается просмотреть страницу пользователя без указания его данных");
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
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Warning(ex, "Ошибка при попытке получить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null||user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", filter.UserId);
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            List<CpcmPost> posts = new List<CpcmPost>();
            if (!user.CpcmIsDeleted || !user.CpcmUserBanned)
            {
                try
                {

                    posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == user.CpcmUserId && c.CpcmPostPublishedDate < DateTime.UtcNow).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
                    if (HttpContext.User.Identity.IsAuthenticated && user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
                    {
                        var friend = await _context.CpcmUserfriends.Where(f => f.CmcpUserId == user.CpcmUserId && f.CmcpFriendId.ToString() == User.FindFirstValue("CpcmUserId")).FirstOrDefaultAsync(); // Тут мы смотрим подал ли ОН запрос в друзья НАМ.                    
                        if (friend != null)
                        {
                            if (friend.CpcmFriendRequestStatus == true)
                                user.IsFriend = FriendStatusEnum.HisApproved; // Будет кнопка удалить из друзей (удалить friendRequest)
                            else if (friend.CpcmFriendRequestStatus == false)
                                user.IsFriend = FriendStatusEnum.HisRejected; // будет кнопка подтвердить запрос в друзья
                            else
                                user.IsFriend = FriendStatusEnum.HisNotAnswered; // будет кнопка подтвердить запрос в друзья

                        }
                        else if (friend == null)
                        {
                            friend = await _context.CpcmUserfriends.Where(f => f.CmcpUserId.ToString() == User.FindFirstValue("CpcmUserId") && f.CmcpFriendId == user.CpcmUserId).FirstOrDefaultAsync();// Теперь мы смотрим подали ли МЫ запрос в друзья ЕМУ.
                            if (friend != null)
                            {
                                if (friend.CpcmFriendRequestStatus == true)
                                    user.IsFriend = FriendStatusEnum.OurApproved; // будет кнопка удалить из друзей
                                else if (friend.CpcmFriendRequestStatus == false)
                                    user.IsFriend = FriendStatusEnum.OurRejected; // будет кнопка отозвать запрос (удалить friendRequest)
                                else
                                    user.IsFriend = FriendStatusEnum.OurNotAnswered; // будет кнопка отозвать запрос (удалить friendRequest)
                            }
                            else
                            {
                                user.IsFriend = FriendStatusEnum.NoFriendRequest; // Будет кнопка добавить в друзья
                            }
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
                        user.IsFriend = FriendStatusEnum.NoFriendRequest;
                        user.IsFollowing = false;
                    }
                }
                catch(DbUpdateException ex)
                {
					Log.Error(ex, "Ошибка при попытке получить посты пользователя из базы данных, а также статура дружбы и подписоты");
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить посты пользователя из базы данных, а также статура дружбы и подписоты");
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                } 
            }
            ICollection<PostModel> postsWithLikesCount = new List<PostModel>();
            UserProfileAndPostsModel userProfile = new();
            userProfile.User = user;
            foreach (var postik in posts)
            {
				postik.User = user;
                long likes = 0;
                long reposts = 0; 
                long liked = 0;
                try
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                    likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
                    reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId} AND CPCM_IsDeleted = 0").CountAsync();
                    if (User.Identity.IsAuthenticated)
                    {
                        liked = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId} AND CPCM_UserID = {User.FindFirstValue("CpcmUserId")}").CountAsync();

                    }
                }
                catch(DbUpdateException ex)
                {
					Log.Error(ex, "Ошибка при попытке получить лайки и репосты поста из базы данных");
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить лайки и репосты поста из базы данных");
                }
				if (liked > 0)
					postik.IsLiked = true;
				else
					postik.IsLiked = false;
				if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
				{
					string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
					if (timezoneOffsetCookie != null)
					{
						if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
						{
							TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);
							
							postik.CpcmPostPublishedDate -=  offset;

						}
					}
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
                Log.Warning("Пользователь {user} не имеет прав на редактирование пользователей", User.FindFirstValue("CpcmUserId"));
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null)
            {
                Log.Warning("Пользователь не найден {user}", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Log.Warning("Пользователь заблокирован {user}", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 403;
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Пользователь был заблокирован";
                return View("UserError");
            }
            if (user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь удалён {user}", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }

            try
            {
                ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
                ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
                ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить данные для формы редактирования пользователя {user} из базы данных", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить данные для формы редактирования пользователя {user} из базы данных", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
                Log.Warning("Пользователь {user} не имеет прав на редактирование пользователей", User.FindFirstValue("CpcmUserId"));
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
                catch(DbUpdateException ex)
                {
					Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                }

                if (cpcmUser == null)
                {
                    Log.Warning("Пользователь не найден {user}", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                if (cpcmUser.CpcmUserBanned)
                {
                    Log.Warning("Пользователь заблокирован {user}", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пользователь был заблокирован";
                    return View("UserError");
                }
                if (cpcmUser.CpcmIsDeleted)
                {
                    Log.Warning("Пользователь удалён {user}", User.FindFirstValue("CpcmUserId"));
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
                            Log.Error(ex, "Ошибка при попытке сохранить изображение {@image} пользователя {user} на сервере", user.CpcmUserImage, User.FindFirstValue("CpcmUserId"));
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
                            Log.Error(ex, "Ошибка при попытке сохранить изображение {@image} пользователя {user} на сервере", user.CpcmUserCoverImage, User.FindFirstValue("CpcmUserId"));
                            cpcmUser.CpcmUserCoverPath = null;
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    try
                    {
                        ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
                        ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
                        ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
                    }
                    catch(DbUpdateException ex)
                    {
						Log.Error(ex, "Ошибка получения списков select для формы редактирования пользователя {user}", User.FindFirstValue("CpcmUserId"));
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
						return View("UserError");
					}
                    catch (DbException ex)
                    {
                        Log.Error(ex, "Ошибка получения списков select для формы редактирования пользователя {user}", User.FindFirstValue("CpcmUserId"));
                        Response.StatusCode = 500;
                        ViewData["ErrorCode"] = 500;
                        ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                        return View("UserError");
                    }
                    return View(user);
                }


                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Log.Warning(ex,"Пользователь {user} был изменён другим пользователем", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 409;
                    ViewData["ErrorCode"] = 409;
                    ViewData["Message"] = "Пользователь был изменён ранее. Пожалуйста, обновите страницу и попробуйте снова";
                    return View("UserError");
                }
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке сохранить изменения пользователя {user} в базе данных", User.FindFirstValue("CpcmUserId"));
                    try
                    {
                        if (System.IO.File.Exists(filePathUserImage))
                        {
                            System.IO.File.Delete(filePathUserImage);
                        }
                        if (System.IO.File.Exists(filePathUserCoverImage))
                        {
                            System.IO.File.Delete(filePathUserCoverImage);
                        }
                    }
                    catch (IOException exx)
                    {
                        Log.Error(exx, "Ошибка при попытке удалить изображения пользователя {user} с сервера", User.FindFirstValue("CpcmUserId"));
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

            try
            {
                ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName", user.CpcmUserCity);
                ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName", user.CpcmUserSchool);
                ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName", user.CpcmUserUniversity);
            }
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Ошибка получения списков select для формы редактирования пользователя {user}", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Ошибка получения списков select для формы редактирования пользователя {user}", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            return View(user);
        }




        [Authorize]
        [HttpPost]
        public async Task<ActionResult> EditIdentity(string id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                Log.Warning("Пользователь {user} не имеет прав на редактирование пользователей", User.FindFirstValue("CpcmUserId"));
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
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден {user}", id);
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            if (user.CpcmUserBanned)
            {
                Log.Warning("Пользователь заблокирован {user}", User.FindFirstValue("CpcmUserId"));
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
                    Log.Warning("Пользователь {user} не имеет прав на редактирование пользователей", User.FindFirstValue("CpcmUserId"));
                    return StatusCode(403);
                }


                CpcmUser cpcmUser;
                try
                {
                    cpcmUser = await _context.CpcmUsers.Where(c => c.CpcmUserId == Guid.Parse(user.CpcmUserId.ToString())).FirstOrDefaultAsync();
                }
                catch(DbUpdateException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить пользователя {user} из базы данных ", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                    return View("UserError");
                }

                if (cpcmUser == null)
                {
                    Log.Warning("Пользователь не найден {user}", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Пользователь не найден";
                    return View("UserError");
                }
                if (cpcmUser.CpcmUserBanned)
                {
                    Log.Warning("Пользователь заблокирован {user}", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 403;
                    ViewData["ErrorCode"] = 403;
                    ViewData["Message"] = "Пользователь был заблокирован";
                    return View("UserError");
                }
                if (cpcmUser.CpcmIsDeleted)
                {
                    Log.Warning("Пользователь удалён {user}", User.FindFirstValue("CpcmUserId"));
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
                catch (DbUpdateConcurrencyException ex)
                {
                    Log.Error(ex, "Пользователь {user} был изменён другим пользователем", User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 409;
                    ViewData["ErrorCode"] = 409;
                    ViewData["Message"] = "Пользователь был изменён ранее. Пожалуйста, обновите страницу и попробуйте снова";
                    return View("UserError");
                }
                catch(DbUpdateException ex)
                {
                    Log.Fatal(ex, "Ошибка при попытке сохранить изменения пользователя {user} в базе данных", User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("UserError");
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
            Log.Warning("Ошибка валидации формы редактирования пользователя {user}", User.FindFirstValue("CpcmUserId"));
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BanUnbanUser(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
                Log.Warning("Пользователь {user} не имеет прав на редактирование пользователей", User.FindFirstValue("CpcmUserId"));
                return StatusCode(403);
            }
            try
            {
                var user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id && c.CpcmIsDeleted==false).FirstOrDefaultAsync();
                if (user == null || user.CpcmIsDeleted)
                {
                    Log.Warning("Пользователь не найден {user}", id);
                    return StatusCode(404);
                }
                user.CpcmUserBanned = !user.CpcmUserBanned;
                await _context.SaveChangesAsync();
                Log.Information("Пользователь {user} заблокирован/разблокирован", User.FindFirstValue("CpcmUserId"));
                return StatusCode(200 , new {status=true});
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex,"Пользователь {user} был изменён другим пользователем", User.FindFirstValue("CpcmUserId"));
                return StatusCode(409, new {message = "Пользователь был изменён другим пользователем" });
            }
            catch(DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке заблокировать/разблокировать пользователя {user}", User.FindFirstValue("CpcmUserId"));
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке заблокировать/разблокировать пользователя {user}", User.FindFirstValue("CpcmUserId"));
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
				ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName");
				ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName");
			}
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
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
            if(filters.UserRole.HasValue && CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
				friendList1 = friendList1.Where(u => u.CpcmUserRole==filters.UserRole);
			}
            try
            {
                var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
                return View(result);
            }
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			


        }
        [HttpPost]
        public async Task<ActionResult> GetNextFriends(UserFilterModel filters)
        {
            CpcmUser user = null;
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			catch (DbException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				return StatusCode(500);
			}


            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
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
            if(filters.UserRole.HasValue && CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
                friendList1 = friendList1.Where(u => u.CpcmUserRole==filters.UserRole);
            }
            try
            {
                var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
				return PartialView(result);
			}
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список друзей пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			
        }





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
            catch(DbUpdateException ex)
            {
				Log.Error(ex,"Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
				ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName");
				ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName");
			}
            catch (DbException ex)
            {
                Log.Error(ex,"Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", filters.UserId);
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
            if(CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
				followerList1 = followerList1.Where(u => u.CpcmUserRole==filters.UserRole);
			}
            try
            {
                var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();

                return View(result);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
                Response.StatusCode = 500;
                return StatusCode(500);
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", user.CpcmUserId);
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
            if(filters.UserRole.HasValue && CheckUserPrivilege("CpcmCanEditUsers", "True"))
            {
                followerList1 = followerList1.Where(u => u.CpcmUserRole==filters.UserRole);
            }
            try
            {
                var result = await followerList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
                return PartialView(result);
            }
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список подписчиков пользователя из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (user == null || user.CpcmIsDeleted)
			{
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
				ViewData["CpcmGroupCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список групп пользователя из базы данных {u}", user.CpcmUserId);
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

            try
            {

                var result = await groupsList.OrderBy(p => p.CpcmGroupId).Take(10).ToListAsync();
                return View(groupsList);
            }
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список групп пользователя из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
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
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", filters.UserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}

			if (user == null || user.CpcmIsDeleted)
			{
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
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
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список групп пользователя из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список групп пользователя из базы данных {u}", user.CpcmUserId);
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


            try
            {
                var result = await groupsList.OrderBy(p => p.CpcmGroupId).Take(10).ToListAsync();
                return PartialView(groupsList);
            }
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список групп пользователя из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
		}

		[Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", id))
            {
                Log.Warning("Пользователь {user} не имеет прав на удаление пользователя {u}",User.FindFirstValue("CpcmUserId"), id);
                return StatusCode(403);
            }

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
			{
                Log.Warning("Пользователь не найден или удалён {u}", id);
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            //if (user.CpcmUserBanned)
            //{
            //    Log.Warning("Пользователь заблокирован и не может быть удалён {u}", user.CpcmUserId);
            //    Response.StatusCode = 403;
            //    ViewData["ErrorCode"] = 403;
            //    ViewData["Message"] = "Пользователь заблокирован и не может быть удалён";
            //    return View("UserError");
            //}
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(UserDeleteModel userdel)
        {
            if (!CheckUserPrivilege("CpcmCanEditUsers", "True", userdel.CpcmUserId))
            {
                Log.Error("Пользователь {user} не имеет прав на удаление пользователя {u}", User.FindFirstValue("CpcmUserId"), userdel.CpcmUserId);
                return StatusCode(403);
            }

            CpcmUser user;
            try
            {
                user = await _context.CpcmUsers.Where(c => c.CpcmUserId == userdel.CpcmUserId).FirstOrDefaultAsync();
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }

            if (user == null || user.CpcmIsDeleted)
            {
                Log.Warning("Пользователь не найден или удалён {u}", userdel.CpcmUserId);
                Response.StatusCode = 404;
                ViewData["ErrorCode"] = 404;
                ViewData["Message"] = "Пользователь не найден";
                return View("UserError");
            }
            //if (user.CpcmUserBanned)
            //{
            //    Log.Warning("Пользователь заблокирован и не может быть удалён {u}", user.CpcmUserId);
            //    Response.StatusCode = 403;
            //    ViewData["ErrorCode"] = 403;
            //    ViewData["Message"] = "Пользователь заблокирован и не может быть удалён";
            //    return View("UserError");
            //}
            //_context.CpcmUsers.Remove(user);
            user.CpcmIsDeleted = true;

            try
            {
                await _context.SaveChangesAsync();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch(DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Ошибка при попытке удалить пользователя из базы данных");
                Response.StatusCode = 409;
                ViewData["ErrorCode"] = 409;
                ViewData["Message"] = "Пользователь был изменён другим пользователем. Повторите попытку позже";
                return View("UserError");
            }
            catch (DbUpdateException ex)
            {
                Log.Fatal(ex,"Ошибка при попытке удалить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке удалить пользователя из базы данных");
                Response.StatusCode = 500;
                ViewData["ErrorCode"] = 500;
                ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                return View("UserError");
            }
            
            return RedirectToAction("UserLogIn", "Index");

        }

		[Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(Guid CpcmUserId)
        {

            CpcmUserfollower follower = new();
            follower.CpcmFollowersId = Guid.NewGuid();
            follower.CpcmUserId = CpcmUserId;
            follower.CpcmFollowerId = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            
			try
            {
                Guid id = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
                var user = await _context.CpcmUsers.FindAsync(id);
				if (user==null)
				{
					Log.Warning("Пользователь не найден", CpcmUserId);
					return StatusCode(404);
				}
                else if(user.CpcmIsDeleted || user.CpcmUserBanned)
                {
                    Log.Warning("Пользователь удалён или заблокирован", CpcmUserId);
                    return StatusCode(403);
                }

				var follow = await _context.CpcmUserfollowers.Where(e => e.CpcmUserId == CpcmUserId && e.CpcmFollowerId == id).Include(e => e.CpcmUser).FirstOrDefaultAsync();

                if (follow == null)
                {
                    _context.CpcmUserfollowers.Add(follower);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Log.Warning("Пользователь уже подписан на этого человека {u}", CpcmUserId);
                    return StatusCode(200, new { status = false, message = "Вы уже подписаны на этого человека" });
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Ошибка при попытке добавить подписчика в базу данных - конукуренция");
                return StatusCode(409);
            }
            catch(DbUpdateException ex)
            {
                Log.Fatal(ex, "Ошибка при попытке добавить подписчика в базу данных");
                return StatusCode(500);
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке добавить подписчика в базу данных");
                return StatusCode(500);
            }

			return StatusCode(200, new { status = true });
		}

		[Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(Guid CpcmUserId)
        {
            CpcmUserfollower? follow;
            try
            {
                follow = await _context.CpcmUserfollowers.Where(e => e.CpcmUserId == CpcmUserId).FirstOrDefaultAsync();
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить подписчика из базы данных");
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить подписчика из базы данных");
                return StatusCode(500);
            }


            if (follow == null)
            {
                Log.Warning("Подписчик не найден {u}", CpcmUserId);
                return StatusCode(404);
            }

            _context.Remove(follow);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Ошибка при попытке удалить подписчика из базы данных - конукуренция");
                return StatusCode(409);
            }
            catch(DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке удалить подписчика из базы данных");
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке удалить подписчика из базы данных");
                return StatusCode(500);
            }

            return StatusCode(200, new {status=true});

        }




        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFriendRequest(Guid CpcmUserId)
        {
            //CpcmUserfriend friendRequest = new();
            //friendRequest.CmcpUserId = Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            //friendRequest.CmcpFriendId = CpcmUserId;
            var userGuid= Guid.Parse(HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            //_context.CpcmUserfriends.Add(friendRequest);
            CpcmUserfriend? friendreq = null;

			try
            {
				var user = await _context.CpcmUsers.FindAsync(CpcmUserId);
				if (user == null)
				{
					Log.Warning("Пользователь не найден", CpcmUserId);
					return StatusCode(404);
				}
				else if (user.CpcmIsDeleted || user.CpcmUserBanned)
				{
					Log.Warning("Пользователь удалён или заблокирован", CpcmUserId);
					return StatusCode(403);
				}


				friendreq = _context.CpcmUserfriends.Where( f => f.CmcpUserId==userGuid && f.CmcpFriendId==CpcmUserId || f.CmcpUserId == CpcmUserId && f.CmcpFriendId == userGuid).FirstOrDefault();
                if(friendreq != null)
                {
                    Log.Warning("Попытка создать запрос на дружбу когда уже есть заявка {u}", CpcmUserId);
                    return StatusCode(200, new {status=false, message="У вас уже есть запрос на дружбу от этого человека"});
                }
                CpcmUserfriend friendRequest = new() { CmcpUserId=userGuid, CmcpFriendId=CpcmUserId};
				_context.CpcmUserfriends.Add(friendRequest);
				await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Ошибка при попытке добавить запрос на дружбу в базу данных - конукуренция");
                return StatusCode(409);
            }
            catch(DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке добавить запрос на дружбу в базу данных {@fr}", friendreq);
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке добавить запрос на дружбу в базу данных");
                return StatusCode(500);
            }

            return StatusCode(200, new { status = true });
        }

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AnswerToFriendRequests(Guid CpcmUserId, bool status)
        {
            CpcmUserfriend? friendRequest;
            try
            {
                var guidString = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value;
                friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId == CpcmUserId
                    && c.CmcpFriendId.ToString() == guidString).FirstOrDefaultAsync(); //Тут мы смотрим только те реквесты, которые адресованы нам. 
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить запрос на дружбу из базы данных");
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить запрос на дружбу из базы данных");
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
			catch (DbUpdateConcurrencyException ex)
			{
				Log.Error(ex, "Ошибка при попытке обновить запрос на дружбу из базы данных {@fr}",friendRequest);
				return StatusCode(500);
			}
            catch (DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке обновить запрос на дружбу из базы данных {@fr}", friendRequest);
				return StatusCode(500);
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке обновить запрос на дружбу из базы данных");
                return StatusCode(500);
            }

            return StatusCode(200, new { status = true });
        }

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFriendRequests(Guid CpcmUserId)
        {
            CpcmUserfriend? friendRequest;
			try
			{
				var guid = HttpContext.User.FindFirst(d => d.Type == "CpcmUserId").Value;

				friendRequest = await _context.CpcmUserfriends.Where(c => c.CmcpUserId.ToString() == guid && c.CmcpFriendId == CpcmUserId
                || c.CmcpUserId == CpcmUserId && c.CmcpFriendId.ToString() == guid).FirstOrDefaultAsync();
            }
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить запрос на дружбу из базы данных");
                return StatusCode(500);
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить запрос на дружбу из базы данных");
                return StatusCode(500);
            }
            if (friendRequest == null)
            {
                Log.Warning("Запрос на дружбу не найден {u}", CpcmUserId);
                return StatusCode(404);
            }
            _context.CpcmUserfriends.Remove(friendRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
				Log.Error(ex, "Ошибка при попытке удалить запрос на дружбу из базы данных");
				return StatusCode(409);
			}
			catch (DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке удалить запрос на дружбу из базы данных");
				return StatusCode(500);
			}
			catch (DbException ex)
            {
				Log.Error(ex, "Ошибка при попытке удалить запрос на дружбу из базы данных");
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
				var stringGuid = User.FindFirstValue("CpcmUserId");
				user = await _context.CpcmUsers.Where(c => c.CpcmUserId.ToString() == stringGuid).FirstOrDefaultAsync();
			}
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			if (user == null || user.CpcmIsDeleted)
			{
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
				Response.StatusCode = 404;
				return StatusCode(404);
			}
            if (user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
            {
                Log.Warning("Пользователь не имеет прав на просмотр запросов на дружбу {u}", user.CpcmUserId);
                Response.StatusCode = 403;

                return StatusCode(403);
            }
			IQueryable<CpcmUser> friendList1;
			try
			{
				friendList1 = _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && (c.CpcmFriendRequestStatus == null)).Select(c => c.CmcpUser);//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
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

            try
            {
                var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
                //followerList1.AddRange(followerList2);

                return View(result);
            }
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
		}
        [Authorize]
		public async Task<IActionResult> GetNextFriendRequests(UserFilterModel filters)
		{
			CpcmUser user = null;
			try
			{
				var stringGuid = User.FindFirstValue("CpcmUserId");
				user = await _context.CpcmUsers.Where(c => c.CpcmUserId.ToString() == stringGuid).FirstOrDefaultAsync();
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить пользователя из базы данных {u}", User.FindFirstValue("CpcmUserId"));
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			if (user == null || user.CpcmIsDeleted)
			{
                Log.Warning("Пользователь не найден или удалён {u}", filters.UserId);
				Response.StatusCode = 404;
				return StatusCode(404);
			}
			if (user.CpcmUserId.ToString() != User.FindFirstValue("CpcmUserId"))
			{
                Log.Warning("Пользователь не имеет прав на просмотр запросов на дружбу {u}", user.CpcmUserId);
				Response.StatusCode = 403;

				return StatusCode(403);
			}
			IQueryable<CpcmUser> friendList1;
			try
			{
				friendList1 = _context.CpcmUserfriends.Where(c => c.CmcpFriendId == user.CpcmUserId && (c.CpcmFriendRequestStatus == null) && c.CmcpUserId.CompareTo(filters.lastId) > 0).Select(c => c.CmcpUser);//followerList2 = await _context.CpcmUserfollowers.Where(c => c.CpcmFollowerId == user.CpcmUserId).Select(c => c.CpcmUser).ToListAsync();
			}
            catch(DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
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

            try
            {
                var result = await friendList1.OrderBy(p => p.CpcmUserId).Take(10).ToListAsync();
                //followerList1.AddRange(followerList2);

                return PartialView(result);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
			catch (DbException ex)
			{
                Log.Error(ex, "Ошибка при попытке получить список запросов на дружбу из базы данных {u}", user.CpcmUserId);
				Response.StatusCode = 500;
				return StatusCode(500);
			}
		}





		[Authorize]
        public async Task<IActionResult> CreatePost()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
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
                    Log.Warning("Попытка создать пустой {@post} пост {u}",userPost, User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 400;
					ViewData["ErrorCode"] = 400;
					ViewData["Message"] = "Нельзя создавать пустой пост";
					return View("CreatePost",userPost);
				}
                CpcmPost post = new CpcmPost();

                post.CpcmPostText = userPost.Text.Trim();
                post.CpcmPostId = Guid.NewGuid();
                if (userPost.PostFatherId!=null)
                {
                    var fatherPost = await _context.CpcmPosts.FindAsync(userPost.PostFatherId);
                    if (fatherPost == null)
                        return StatusCode(StatusCodes.Status400BadRequest);
                    if (fatherPost.CpcmUserId == Guid.Parse(User.FindFirst(c => c.Type == "CpcmUserId").Value))
                        return StatusCode(StatusCodes.Status417ExpectationFailed);
                }
                post.CpcmPostFather = userPost.PostFatherId;
                post.CpcmPostCreationDate = DateTime.UtcNow;
                if (userPost.Published == null)
                {
					post.CpcmPostPublishedDate = post.CpcmPostCreationDate;
				}
                else
                {

					post.CpcmPostPublishedDate = userPost.Published;
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate += offset;

							}
						}
					}
				}
                
                post.CpcmUserId = Guid.Parse(User.FindFirst(c => c.Type == "CpcmUserId").Value);
 

                List<string> filePaths = new List<string>();
                List<CpcmImage> images = new List<CpcmImage>();

                if (userPost.PostFatherId == null)
                {
                    int i = 0;
                    if (userPost.Files != null)
                    {
                        foreach (IFormFile file in userPost.Files)
                        {
                            CheckIFormFile("Files", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                            if (!ModelState.IsValid)
                            {
                                Log.Warning("Попытка создать пост {@Files} с некорректными файлами {u}",userPost.Files ,User.FindFirstValue("CpcmUserId"));
                                return View("CreatePost", userPost);
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
                            catch (IOException ex)
                            {
                                Log.Error(ex, "Ошибка при попытке сохранить файл {u}", User.FindFirstValue("CpcmUserId"));
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
                                return View("CreatePost", userPost);
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
                        var fatheruser = await _context.CpcmUsers.FindAsync(fatherPost.CpcmUserId);
                        var fathergroup = await _context.CpcmGroups.FindAsync(fatherPost.CpcmGroupId);
                        if(fatherPost==null || fatherPost.CpcmPostPublishedDate > DateTime.UtcNow)
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

                        if (fatheruser == null)
                        {
							return StatusCode(404);
						}
                        if(fatheruser!=null &&  fatheruser.CpcmIsDeleted || fatheruser.CpcmUserBanned)
                        {
							return StatusCode(403);
						}

                        if(fathergroup == null)
                        {
							return StatusCode(404);
						}
                        if(fathergroup!=null && fathergroup.CpcmIsDeleted || fathergroup.CpcmGroupBanned)
                        {
                            return StatusCode(403);
                        }

						
					}
                    await _context.SaveChangesAsync();
                    if (userPost.PostFatherId != null)
                    {
                        try
                        {
                            var querry = await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO CPCM_POSTREPOSTS VALUES ({post.CpcmPostFather},{post.CpcmUserId}, 0)");
                            if (querry == 1)
                            {
                                return StatusCode(200, new { status = true });
                            }
                            else
                            {
                                return StatusCode(500, new { message = "Не удалось установить соединение с сервером" });
                            }
                        }
                        catch (SqlException ex)
                        {
                            if(ex.Number == 2627)
                            {
                                //Репост поста который уже репостнули. Игнорим.
                            }
                            else
                            {
                                Log.Error(ex, "Ошибка при попытке сохранить репост {post} {u}", post, User.FindFirstValue("CpcmUserId"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Fatal(ex, "Ошибка при попытке сохранить репост {post} {u}", post, User.FindFirstValue("CpcmUserId"));
                        }
                    }
				}
                catch (DbUpdateConcurrencyException ex)
                {
                    Log.Error(ex, "Ошибка при попытке сохранить пост {post} {u} - конкуренция",userPost, User.FindFirstValue("CpcmUserId"));
                    try
                    {
                        foreach (var item in filePaths)
                        {
                            if (System.IO.File.Exists(item))
                            {
                                System.IO.File.Delete(item);
                            }
                        }
                    }
                    catch (IOException exx)
                    {
                        Log.Error(exx, "Не удалось удалить файлы {@files}", filePaths);
                    }
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("CreatePost", userPost); // TODO Продумать место для сохранения еррора
				}
                catch(DbUpdateException ex)
                {
					Log.Fatal(ex, "Ошибка при попытке сохранить пост {post} {u}", userPost, User.FindFirstValue("CpcmUserId"));
					try
					{
						foreach (var item in filePaths)
						{
							if (System.IO.File.Exists(item))
							{
								System.IO.File.Delete(item);
							}
						}
					}
					catch (IOException exx)
					{
						Log.Error(exx, "Не удалось удалить файлы {@files}", filePaths);
					}
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
					return View("CreatePost", userPost); // TODO Продумать место для сохранения еррора
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке сохранить пост {post} {u}", userPost, User.FindFirstValue("CpcmUserId"));
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
                    return View("CreatePost", userPost); // TODO Продумать место для сохранения еррора
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
            Log.Information("Пост {@userPost} не прошёл валидацию {u}", userPost,User.FindFirstValue("CpcmUserId"));
            if (userPost.PostFatherId == null)
            {
                return View("CreatePost", userPost); 
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
            bool repost = false;
            try
            {
                post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).FirstOrDefaultAsync();
                if (post.CpcmPostFather != null)
                {
                    repost = true;
                }
            }
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Ошибка при попытке получить пост из базы данных");
				return StatusCode(500);
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пост из базы данных");
                return StatusCode(500);
            }
            if (post == null || post.CpcmIsDeleted)
            {
                Log.Warning("Пост не найден или удалён {u}", postGuid);
                return StatusCode(404);
            }
            //if (post.CpcmPostBanned)
            //{
            //    Log.Warning("Пост заблокирован и не может быть удалён {u}", postGuid);
            //    return StatusCode(403);
            //}

            if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()))
            {
                Log.Warning("Пользователь {user} не имеет прав на удаление поста {u}",User.FindFirstValue("CpcmUserId"), postGuid);
                return StatusCode(403);
            }


            post.CpcmIsDeleted = true;
            try
            {
                using(var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE CPCM_POSTREPOSTS SET CPCM_IsDeleted = 1 WHERE CPCM_PostID={postGuid} AND CPCM_UserID={User.FindFirstValue("CpcmUserId")}");
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
						await transaction.RollbackAsync();
                        Log.Error(ex, "Ошибка при попытке удалить пост из базы данных {post}",postGuid);
						return StatusCode(500);
					}
				}
                
            }
            catch (DbUpdateConcurrencyException ex)
            {
				Log.Error(ex, "Ошибка при попытке удалить пост из базы данных {post}",postGuid);
				return StatusCode(409);
			}
			catch (DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при попытке удалить пост из базы данных {post}",postGuid);
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке удалить пост из базы данных {post}",postGuid);
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
				post = await _context.CpcmPosts.Where(c => c.CpcmPostId == postGuid).Include(c => c.CpcmImages).FirstOrDefaultAsync();
			}
			catch (DbUpdateException ex)
			{
				Log.Error(ex, "Ошибка при попытке получить пост из базы данных {guid}", postGuid);
				return StatusCode(500);
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить пост из базы данных {guid}",postGuid);
                return StatusCode(500);
            }

            if (post == null)
            {
                Log.Warning("Пост не найден {u}", postGuid);
                return StatusCode(404);
            }
            if (post.CpcmIsDeleted)
            {
                Log.Warning("Пост удалён {u}", postGuid);
                return StatusCode(404);
            }
            if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()) || post.CpcmPostBanned == true)
            {
                Log.Warning("Пользователь не имеет прав на редактирование поста {u}", postGuid);
                return View("Index");
            }
            
            
            UserPostEditModel model = new UserPostEditModel();
            model.Id = post.CpcmPostId;
            model.UserId = post.CpcmUserId;
            model.Text = post.CpcmPostText;
            model.CpcmImages = post.CpcmImages;
            model.NewPublishDate = post.CpcmPostPublishedDate;
			if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
			{
				string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
				if (timezoneOffsetCookie != null)
				{
					if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
					{
						TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

						model.NewPublishDate -= offset;

					}
				}
			}
			return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPostP(UserPostEditModel editPost)
        {
            //if (editPost.Text == null && editPost.FilesToDelete.Count == 0 && editPost.NewFiles.Count == 0)
            //{
            //    return View(editPost);
            //}

            Log.Debug("Попытка редактирования поста {@post} {u}", editPost, User.FindFirstValue("CpcmUserId"));
            if (ModelState.IsValid)
            {
                Log.Debug("Пост прошёл валидацию {@post} {u}",editPost, User.FindFirstValue("CpcmUserId"));
                CpcmPost? post = null;
                try
                {
                    post = await _context.CpcmPosts.Include(c => c.CpcmImages).Where(c => c.CpcmPostId == editPost.Id).FirstOrDefaultAsync();

				}
                catch(DbUpdateException ex)
                {
					Log.Error(ex, "Ошибка при попытке получить пост из базы данных {post}", editPost.Id);
					return StatusCode(500);
				}
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке получить пост из базы данных {post}",editPost.Id);
                    return StatusCode(500);
                }
                if (post == null)
                {
                    Log.Warning("Пост не найден {u}", editPost.Id);
                    return StatusCode(404);
                }
                if (post.CpcmIsDeleted)
                {
                    Log.Warning("Пост удалён {u}", editPost.Id);
                    return StatusCode(404);
                }
                if (!CheckUserPrivilege("CpcmCanDelUsersPosts", "True", post.CpcmUserId.ToString()) || post.CpcmPostBanned == true)
                {
                    Log.Warning("Пользователь не имеет прав на редактирование поста {u}", editPost.Id);
                    return StatusCode(403);
                }
				editPost.CpcmImages = post.CpcmImages;
				if (post.CpcmImages.Count - editPost.FilesToDelete.Count + editPost.NewFiles.Count > 4)
                {
                    //Log.Debug("Попытка добавить больше 4 файлов в пост {u}", editPost.Id);
                    Log.Warning("Попытка добавить больше 4 файлов в пост {u}", editPost.Id);
                    ModelState.AddModelError("NewFiles", "В посте не может быть больше 4 фотографий");
                    return View("EditPost", editPost);
                }

                post.CpcmPostText = editPost.Text.Trim();

				if (editPost.NewPublishDate == null)
				{
					//post.CpcmPostPublishedDate = DateTime.UtcNow;
				}
				else
				{
					post.CpcmPostPublishedDate = editPost.NewPublishDate;
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								post.CpcmPostPublishedDate += offset;

							}
						}
					}
				}

				//post.CpcmPostPublishedDate = DateTime.UtcNow;
                if(post.CpcmPostFather != null)
                {
                    Log.Debug("Создаётся репост {@post}", editPost);
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
                    Log.Debug("Попытка добавить файл {@file} в пост {u}",file, editPost.Id);

                    CheckIFormFile("NewFiles", file, 8388608, new[] { "image/jpeg", "image/png", "image/gif" });

                    if (!ModelState.IsValid)
                    {
                        Log.Debug("Файл {@file} не прошёл валидацию {u}",file, User.FindFirstValue("CpcmUserId"));
                        return View("EditPost", editPost);
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
                    catch (IOException ex)
                    {
                        Log.Error(ex, "Ошибка при попытке сохранить файл {u}", User.FindFirstValue("CpcmUserId"));
                        foreach (var uploadedfile in filePaths)
                        {
                            if (System.IO.File.Exists(uploadedfile)){
                                System.IO.File.Delete(uploadedfile);
                            }
                        }
                        Response.StatusCode = 500;
                        ViewData["ErrorCode"] = 500;
                        ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
                        return View("EditPost", editPost);
                    }

                }


                List<CpcmImage>? images = post.CpcmImages.Where(c => editPost.FilesToDelete.Contains(c.CpcmImageId)).ToList();
                if (images!=null&&images.Count != 0)
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
                    catch(IOException ex)
                    {
                        Log.Error(ex, "Ошибка при попытке удалить файлы {u}", User.FindFirstValue("CpcmUserId"));
						Response.StatusCode = 500;
						ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
						return View("UserError");
					}
                    catch(DbUpdateConcurrencyException ex)
                    {
                        Log.Error(ex, "Ошибка при попытке сохранить пост {@post} {u} - конкуренция",editPost, User.FindFirstValue("CpcmUserId"));
                        foreach (var path in filePaths)
                        {
							if (System.IO.File.Exists(path))
                            {
								System.IO.File.Delete(path);
							}
						}
						Response.StatusCode = 409;
						ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
						return View("UserError");
					}
                    catch(DbUpdateException ex)
                    {
						Log.Error(ex, "Ошибка при попытке сохранить пост {@post} {u} - конкуренция", editPost, User.FindFirstValue("CpcmUserId"));
						foreach (var path in filePaths)
						{
							if (System.IO.File.Exists(path))
							{
								System.IO.File.Delete(path);
							}
						}
						Response.StatusCode = 500;
						ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
						return View("UserError");
					}
                    catch (DbException ex)
                    {
                        Log.Error(ex, "Ошибка при попытке сохранить пост {@post} {u}",editPost, User.FindFirstValue("CpcmUserId"));
                        foreach (var path in filePaths)
                        {
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                        }
						Response.StatusCode = 500;
						ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
						return View("UserError");
					}
                }


                try
                {
                    if(post.CpcmPostText == null && (await _context.CpcmImages.Where(p => p.CpcmPostId == post.CpcmPostId).ToListAsync()).Count == 0)
                    {
                        Log.Error("Попытка создать пустой пост {u}", User.FindFirstValue("CpcmUserId"));
                        ModelState.AddModelError("Text", "Нельзя создать пустой пост");
                        return View("UserError");
                    }
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    Log.Error(ex, "Ошибка при попытке сохранить пост {@post} {u} - конкуренция",editPost, User.FindFirstValue("CpcmUserId"));
					Response.StatusCode = 409;
                    ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                    return View("UserError");
                }
                catch (DbException ex)
                {
                    Log.Error(ex, "Ошибка при попытке сохранить пост {@post} {u}",editPost, User.FindFirstValue("CpcmUserId"));
                    Response.StatusCode = 500;
                    ViewData["Message"] = "Не удалось сохранить пост. Пожалуйста, повторите запрос позднее или обратитесь к Администратору.";
                    return View("UserError"); // TODO Продумать место для сохранения еррора
                }
                return RedirectToAction("Index");

            }
            return View("EditPost", editPost);
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
                    Log.Warning("Пост не найден {u}", lastPostId);
                    return StatusCode(404);
                }
                var user = await _context.CpcmUsers.Where(c => c.CpcmUserId == userId).FirstOrDefaultAsync();
                if(user!=null && user.CpcmIsDeleted)
                {
                    Log.Warning("Пользователь не найден или удалён {u}", userId);
					return StatusCode(404);
				}
                if(user != null && user.CpcmUserBanned)
                {
                    Log.Warning("Пользователь заблокирован {u}", userId);
					return StatusCode(403);
				}   
                posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == userId).Where(c => c.CpcmPostPublishedDate < post.CpcmPostPublishedDate && c.CpcmPostPublishedDate < DateTime.UtcNow && !post.CpcmIsDeleted).Take(10).ToListAsync();
                foreach (var postik in posts)
                {
                    long likes = 0;
                    long reposts = 0;
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
                    likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = {postik.CpcmPostId}").CountAsync();
                    reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = {postik.CpcmPostId} AND CPCM_IsDeleted = 0").CountAsync();
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
					postModels.Add(new() { Post = postik, LikesCount = likes, RepostsCount = reposts });

                }
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить посты из базы данных {post} {u}", lastPostId, userId);
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
                Log.Warning("Пользователь {uu} не имеет прав на просмотр неопубликованных постов {u}",User.FindFirstValue("CpcmUserId"), userId);
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
                    Log.Warning("Пост не найден {u}", lastPostId);
                    return StatusCode(404);
                }
				var user = await _context.CpcmUsers.Where(c => c.CpcmUserId == userId).FirstOrDefaultAsync();
				if (user != null && user.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь не найден или удалён {u}", userId);
					return StatusCode(404);
				}
				if (user != null && user.CpcmUserBanned)
				{
                    Log.Warning("Пользователь заблокирован {u}", userId);
					return StatusCode(403);
				}
				posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == userId && c.CpcmPostId == lastPostId).Where(c => c.CpcmPostPublishedDate < lastPost.CpcmPostPublishedDate && c.CpcmPostPublishedDate > DateTime.UtcNow && !c.CpcmIsDeleted).Take(10).ToListAsync();
                foreach (var postik in posts)
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
					//long likes = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTLIKES WHERE CPCM_PostID = '{postik.CpcmGroupId}'");
					//long reposts = await _context.Database.SqlQuery<long>($@"SELECT * FROM CPCM_POSTREPOSTS WHERE CPCM_PostID = '{postik.CpcmGroupId}'");
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
					postModels.Add(new() { Post = postik, LikesCount = 0, RepostsCount = 0 });
                }
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить посты из базы данных {post} {u}", lastPostId, userId);
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
                Log.Error("Пользователь {uu} не имеет прав на просмотр неопубликованных постов {u}", User.FindFirstValue("CpcmUserId"), id);
                ViewData["ErrorCode"] = 403;
                ViewData["Message"] = "Доступ запрещён";
                return View("UserError");
            }

            List<CpcmPost> posts;
            List<PostModel> postModels = new List<PostModel>();
            try
            {
                posts = await _context.CpcmPosts.Where(c => c.CpcmUserId == id && c.CpcmPostPublishedDate > DateTime.UtcNow && !c.CpcmIsDeleted).Include(c => c.CpcmImages).OrderByDescending(c => c.CpcmPostPublishedDate).Take(10).ToListAsync();
				var user = await _context.CpcmUsers.Where(c => c.CpcmUserId == id).FirstOrDefaultAsync();
				if (user != null && user.CpcmIsDeleted)
				{
                    Log.Warning("Пользователь не найден или удалён {u}", id);
					return StatusCode(404);
				}
				if (user != null && user.CpcmUserBanned)
				{
                    Log.Warning("Пользователь заблокирован {u}", id);
					return StatusCode(403);
				}
				foreach (var postik in posts)
                {
                    postik.CpcmPostFatherNavigation = await GetFatherPostReccurent(postik);
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								postik.CpcmPostPublishedDate -= offset;

							}
						}
					}
					postModels.Add(new() { Post = postik, LikesCount = 0, RepostsCount = 0 });
                }
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке получить посты из базы данных {u}", id);
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
                Log.Warning("Пользователь {uu} не имеет прав на блокировку постов {u}", User.FindFirstValue("CpcmUserId"), id);
                return StatusCode(403);
            }
            try
            {
                var post = await _context.CpcmPosts.Where(c => c.CpcmPostId == id && c.CpcmPostPublishedDate < DateTime.UtcNow).FirstOrDefaultAsync();
                if(post == null || post.CpcmIsDeleted==true)
                {
                    Log.Warning("Пост не найден или удалён {u}", id);
                    return StatusCode(404);
                }
                post.CpcmPostBanned = !post.CpcmPostBanned;
                await _context.SaveChangesAsync();
                return StatusCode(200, new {status=true});
            }
            catch(DbUpdateConcurrencyException ex)
            {
                Log.Error(ex, "Ошибка при попытке заблокировать пост {u}", id);
                return StatusCode(409);
            }
            catch(DbUpdateException ex)
            {
                Log.Fatal(ex, "Ошибка при попытке заблокировать пост {u}", id);
                return StatusCode(500);
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при попытке заблокировать пост {u}", id);
                return StatusCode(500);
            }
        }





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
			if (filters.UserRole.HasValue && CheckUserPrivilege("CpcmCanEditUsers", "True"))
			{
				query = query.Where(u => u.CpcmUserRole == filters.UserRole);
			}
			try
            {
				ViewData["CpcmUserCity"] = new SelectList(await _context.CpcmCities.ToListAsync(), "CpcmCityId", "CpcmCityName");
				ViewData["CpcmUserSchool"] = new SelectList(await _context.CpcmSchools.ToListAsync(), "CpcmSchooldId", "CpcmSchoolName");
				ViewData["CpcmUserUniversity"] = new SelectList(await _context.CpcmUniversities.ToListAsync(), "CpcmUniversityId", "CpcmUniversityName");
				var rez = await query.OrderBy(u => u.CpcmUserId).Take(10).ToListAsync();
                return View(rez);
            }
            catch(DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при поиске пользователя");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Произошла ошибка с доступом к серверу. Если проблема сохранится спустя некоторое время, то обратитесь в техническую поддержку";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при поиске пользователя");
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
			if (filters.UserRole.HasValue && CheckUserPrivilege("CpcmCanEditUsers", "True"))
			{
				query = query.Where(u => u.CpcmUserRole == filters.UserRole);
			}
			try
            {
                var rez = await query.Where(u => u.CpcmUserId.CompareTo(filters.lastId) > 0).OrderBy(u => u.CpcmUserId).Take(10).ToListAsync();
                return PartialView(rez);
            }
            catch (DbUpdateException ex)
            {
				Log.Fatal(ex, "Ошибка при поиске пользователя");
				return StatusCode(500);
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Ошибка при поиске пользователя");
                return StatusCode(500);
            }
        }






        private bool CheckUserPrivilege(string claimType, string claimValue, string id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id || c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
				Log.Information("Привелегии не подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
				return false;
            }
            Log.Information("Привелегии подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
            return true;
        }
        private bool CheckUserPrivilege(string claimType, string claimValue, Guid id)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == "CpcmUserId" && c.Value == id.ToString() || c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
				Log.Information("Привелегии не подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
				return false;
            }
			Log.Information("Привелегии подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
			return true;
        }
        private bool CheckUserPrivilege(string claimType, string claimValue)
        {
            var authFactor = HttpContext.User.FindFirst(c => c.Type == claimType && c.Value == claimValue);
            if (authFactor == null)
            {
				Log.Information("Привелегии не подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
				return false;
            }
			Log.Information("Привелегии подтверждены {claim} {user}", claimValue, HttpContext.User.FindFirst(c => c.Type == "CpcmUserId").Value);
			return true;
        }

		private async Task<CpcmPost?> GetFatherPostReccurent(CpcmPost cpcmPostFatherNavigation)
		{
			try
			{
				var father = await _context.CpcmPosts.Where(p => p.CpcmPostId == cpcmPostFatherNavigation.CpcmPostFather).Include(p => p.CpcmImages).FirstOrDefaultAsync();
				if (father != null)
				{
					father.CpcmPostFatherNavigation = await GetFatherPostReccurent(father);
					father.User = await _context.CpcmUsers.Where(p => p.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
					father.Group = await _context.CpcmGroups.Where(p => p.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
					if (HttpContext.Request.Cookies.ContainsKey("TimeZone"))
					{
						string timezoneOffsetCookie = HttpContext.Request.Cookies["TimeZone"];
						if (timezoneOffsetCookie != null)
						{
							if (int.TryParse(timezoneOffsetCookie, out int timezoneOffsetMinutes))
							{
								TimeSpan offset = TimeSpan.FromMinutes(timezoneOffsetMinutes);

								father.CpcmPostPublishedDate -= offset;

							}
						}
					}
					var author = await _context.CpcmUsers.Where(u => u.CpcmUserId == father.CpcmUserId).FirstOrDefaultAsync();
					if (author == null)
					{
						var group = await _context.CpcmGroups.Where(u => u.CpcmGroupId == father.CpcmGroupId).FirstOrDefaultAsync();
						father.Group = group;
					}
					else
					{
						father.User = author;
					}
				}
				return father;
			}
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выгрузить родительские посты {@fathrepostnavigation}", cpcmPostFatherNavigation);
				throw;
			}
			catch (DbException ex)
			{
				Log.Error(ex, "Не удалось выгрузить родительские посты {@fathrepostnavigation}", cpcmPostFatherNavigation);
				throw;
			}
		}
		private bool CheckIFormFileContent(IFormFile cpcmUserImage, string[] permittedTypes)//TODO: Объединить с методами при регистрации
        {
            if (cpcmUserImage != null && permittedTypes.Contains(cpcmUserImage.ContentType))
            {
                Log.Information("Файл прошёл валидацию {@file}", cpcmUserImage);
                return true;
            }
            else
            {
                Log.Information("Файл не прошёл валидацию {@file}", cpcmUserImage);
                return false;
            }
        }
        private bool CheckIFormFileSize(IFormFile cpcmUserImage, int size)//TODO: Объединить с методами при регистрации
        {

            if (cpcmUserImage.Length > 0 && cpcmUserImage.Length < size)
            {
                Log.Information("Файл прошёл валидацию размера {size} {@file}",size, cpcmUserImage);
                return true;
            }
            else
            {
                Log.Information("Файл не прошёл валидацию размера {size} {@file}", size, cpcmUserImage);
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
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Не удалось установить соединение с сервером");
				return Json(data: "Не удалось установить соединение с сервером");
			}
            catch (DbException ex)
            {
				//StatusCode(500);
                Log.Error(ex, "Не удалось установить соединение с сервером");
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
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось установить соединение с сервером");
            }
            catch (DbException ex)
            {
				//StatusCode(500);
                Log.Error(ex, "Не удалось установить соединение с сервером");
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
            catch (DbUpdateException ex)
            {
				Log.Error(ex,"Не удалось установить соединение с сервером");
				return Json(data: "Не удалось установить соединение с сервером");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось установить соединение с сервером");
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
                catch (DbUpdateException ex)
                {
					Log.Error(ex,"Не удалось установить соединение с сервером");
					return Json(data: "Не удалось установить соединение с сервером");
				}
                catch (DbException ex)
                {
                    //StatusCode(500);
                    Log.Error(ex, "Не удалось установить соединение с сервером");
					return Json(data: "Не удалось установить соединение с сервером");
				}
               return Json(rez);
            }
        }


    }
}
