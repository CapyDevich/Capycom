﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capycom;
using Microsoft.Extensions.Options;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.Security.Claims;

namespace Capycom.Controllers
{
	[Authorize(Policy = "CanEditRoles")]
	public class GroupRolesController : Controller
    {
		private readonly CapycomContext _context;
		private readonly MyConfig _config;
		private readonly ILogger<GroupRolesController> _logger;

		public GroupRolesController(ILogger<GroupRolesController> logger, CapycomContext context, IOptions<MyConfig> config)
		{
			_context = context;
			_config = config.Value;
			_logger = logger;
		}

		// GET: GroupRoles
		public async Task<IActionResult> Index()
        {
            Log.Information("Пользователь {User} зашел на страницу просмотра ролей. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
            try
            {
                return View(await _context.CpcmGroupRoles.ToListAsync());
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на получения списка ролей");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на получения списка ролей");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // GET: GroupRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    Log.Warning("Пользователь {User} попытался перейти на страницу роли без указания id. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }

                var cpcmGroupRole = await _context.CpcmGroupRoles
                    .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
                if (cpcmGroupRole == null)
                {
                    Log.Warning("Пользователь {User} попытался перейти на страницу роли без указания id. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }
                Log.Information("Пользователь {User} зашел на страницу роли {cpcmGroupRole.CpcmRoleName}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmGroupRole.CpcmRoleName, HttpContext.Connection);
                return View(cpcmGroupRole);
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на получения роли с id {id}", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex,"Не удалось выполнить запрос к базе данных на получения роли с id {id}", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // GET: GroupRoles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GroupRoles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CpcmRoleName,CpcmCanEditGroup,CpcmCanMakePost,CpcmCanDelPost,CpcmCanEditPost")] CpcmGroupRole cpcmGroupRole)
        {
            Log.Information("Пользователь {User} пытается создать новую роль {cpcmGroupRole.CpcmRoleName}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmGroupRole.CpcmRoleName, HttpContext.Connection);
            if (ModelState.IsValid)
            {
                try
                {

                    var lastRole = await _context.CpcmGroupRoles.OrderBy(g => g.CpcmRoleId).FirstOrDefaultAsync();
                    if (lastRole == null)
                    {
                        cpcmGroupRole.CpcmRoleId = 0;

                    }
                    else
                    {
                        cpcmGroupRole.CpcmRoleId = lastRole.CpcmRoleId + 1;
					}
                    _context.Add(cpcmGroupRole);
                    await _context.SaveChangesAsync();
                    Log.Warning("Пользователь {Usere} создал новую роль {cpcmGroupRole.CpcmRoleName}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmGroupRole.CpcmRoleName, HttpContext.Connection);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
					Log.Error(ex, "Не удалось выполнить запрос к базе данных на создание роли {cpcmGroupRole.CpcmRoleName}", cpcmGroupRole.CpcmRoleName);
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
                catch (DbException ex)
                {
                    Log.Error(ex,"Не удалось выполнить запрос к базе данных на создание роли {cpcmGroupRole.CpcmRoleName}", cpcmGroupRole.CpcmRoleName);
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
            }
            Log.Warning("Пользователь {@User} попытался перейти на страницу роли без указания id. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
            return View(cpcmGroupRole);
        }

        // GET: GroupRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log.Information("Пользователь {User} зашел на страницу редактирования роли с id {id}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), id, HttpContext.Connection);
            try
            {
                if (id == null)
                {
                    Log.Warning("Пользователь {User} попытался перейти на страницу редактирования роли без указания id. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }

                var cpcmGroupRole = await _context.CpcmGroupRoles.FindAsync(id);
                if (cpcmGroupRole == null)
                {
                    Log.Warning("Пользователь {User} попытался перейти на страницу редактирования роли с несуществующим id. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }
                return View(cpcmGroupRole);
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на получения роли с id {id}", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на получения роли с id {id}", id);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // POST: GroupRoles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CpcmRoleName,CpcmCanEditGroup,CpcmCanMakePost,CpcmCanDelPost,CpcmCanEditPost")] CpcmGroupRole cpcmGroupRole)
        {
            Log.Information("Пользователь {User} пытается отредактировать роль {id} - изменив на {cpcmGroupROle}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), id, cpcmGroupRole,HttpContext.Connection);
            if (id != cpcmGroupRole.CpcmRoleId)
            {
                Log.Warning("Пользователь {User} попытался изменить id роли. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Роль с данным id не найден";
				return View("UserError");
			}

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cpcmGroupRole);
                    await _context.SaveChangesAsync();
                    Log.Information("Пользователь {User} изменил роль {cpcmGroupRole.CpcmRoleName}. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmGroupRole.CpcmRoleName, HttpContext.Connection);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CpcmGroupRoleExists(cpcmGroupRole.CpcmRoleId))
                    {
                        Log.Error(ex, "Пользователь {User} попытался изменить роль Но данные не удалось сохранить. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
						Response.StatusCode = 404;
						ViewData["ErrorCode"] = 404;
						ViewData["Message"] = "Кто-то до вас удалил данную запись";
						return View("UserError");
					}
                    else
                    {
                        Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли {cpcmGroupRole.CpcmRoleName} поскольку её отредактировали до того, как вы начали отправили форму", cpcmGroupRole.CpcmRoleName);
						Response.StatusCode = 409;
						ViewData["ErrorCode"] = 409;
						ViewData["Message"] = "Ошибка связи с сервером";
						return View("UserError");
					}
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли {cpcmGroupRole.CpcmRoleName}", cpcmGroupRole.CpcmRoleName);
                    Response.StatusCode = 500;
                    ViewData["ErrorCode"] = 500;
                    ViewData["Message"] = "Ошибка связи с сервером";
                }
                catch(DbException ex)
                {
					Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли {cpcmGroupRole.CpcmRoleName}", cpcmGroupRole.CpcmRoleName);
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
				}
                return RedirectToAction(nameof(Index));
            }
            Log.Warning("Пользователь {User} попытался изменить роль на некорректные данные. Данные по соединененю {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection);
            return View(cpcmGroupRole);
        }

        // GET: GroupRoles/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var cpcmGroupRole = await _context.CpcmGroupRoles
        //        .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
        //    if (cpcmGroupRole == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(cpcmGroupRole);
        //}

        //// POST: GroupRoles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var cpcmGroupRole = await _context.CpcmGroupRoles.FindAsync(id);
        //    if (cpcmGroupRole != null)
        //    {
        //        _context.CpcmGroupRoles.Remove(cpcmGroupRole);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool CpcmGroupRoleExists(int id)
        {
            try
            {
                return _context.CpcmGroupRoles.Any(e => e.CpcmRoleId == id);
            }
            catch(DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на проверку существования роли с id {id}", id);
				return true;
			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на проверку существования роли с id {id}", id);
                return true;
            }
        }
    }
}
