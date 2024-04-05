﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capycom;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Serilog;

namespace Capycom.Controllers
{
	[Authorize(Policy = "CanEditRoles")]
	public class RolesController : Controller
    {
		private readonly CapycomContext _context;
		private readonly MyConfig _config;
		private readonly ILogger<RolesController> _logger;

		public RolesController(ILogger<RolesController> logger, CapycomContext context, IOptions<MyConfig> config)
		{
			_context = context;
			_config = config.Value;
			_logger = logger;
		}

		// GET: Roles
		public async Task<IActionResult> Index()
        {
            try
            {
                Log.Information("Пользователь {HttpContext.User.Identity.Name} зашел на страницу Roles. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name,HttpContext.Connection); 
                return View(await _context.CpcmRoles.ToListAsync());
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

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Log.Information("Пользователь {HttpContext.User.Identity.Name} зашел на страницу Roles/Details и попытался просмотреть несуществующую роль без id. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, HttpContext.Connection);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена роль с таким id";
				return View("UserError");
			}

            try
            {
               Log.Information("Пользователь {HttpContext.User.Identity.Name} зашел на страницу Roles/Details и попытался просмотреть роль с id {id}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
                var cpcmRole = await _context.CpcmRoles
                        .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
                if (cpcmRole == null)
                {
                    Log.Information("Пользователь {HttpContext.User.Identity.Name} зашел на страницу Roles/Details и попытался просмотреть роль с id {id}, но такой роли не существует. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Не найдена роль с таким id";
					return View("UserError");
				}
				return View(cpcmRole);
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

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CpcmRoleName,CpcmCanEditUsers,CpcmCanEditGroups,CpcmCanEditRoles,CpcmCanDelUsersPosts,CpcmCanDelUsersComments,CpcmCanDelGroupsPosts,CpcmCanAddPost,CpcmCanAddGroups,CpcmCanAddComments")] CpcmRole cpcmRole)
        {
            Log.Information ("Пользователь {HttpContext.User.Identity.Name} пытается создать роль {cpcmRole}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
            if (ModelState.IsValid)
            {
                try
                {
                    var lastRole= await _context.CpcmRoles.OrderBy(c => c.CpcmRoleId).FirstOrDefaultAsync();
                    if (lastRole != null)
                    {
                        cpcmRole.CpcmRoleId = lastRole.CpcmRoleId + 1;

					}
                    else
                    {
                        cpcmRole.CpcmRoleId = 0;
					}
					_context.Add(cpcmRole);
					await _context.SaveChangesAsync();
                    Log.Information("Пользователю {HttpContext.User.Identity.Name} удалось создать роль {cpcmRole}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
					return RedirectToAction(nameof(Index));
				}
                catch (DbException ex)
                {
                    Log.Error(ex,"Пользователю {HttpContext.User.Identity.Name} удалось выполнить запрос к базе данных на добавление роли {cpcmRole}. Данные по соединению  {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection); 
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
                
            }
            Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался создать роль с некорректными данными {data}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
            return View(cpcmRole);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log.Information("Пользователь {HttpContext.User.Identity.Name} пытается изменить роль с id {id}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
            if (id == null)
            {
                Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить роль без указанного id. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, HttpContext.Connection);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена Роль с таикм id.";
				return View("UserError");
			}

            try
            {
                var cpcmRole = await _context.CpcmRoles.FindAsync(id);
                if (cpcmRole == null)
                {
                    Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить роль с id {id}, но такой роли не существует. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Не найдена Роль с таикм id.";
                    return View("UserError");
                }
                Log.Information("Пользователь {HttpContext.User.Identity.Name} удалось получить роль с id {id}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
                return View(cpcmRole);
            }
            catch (DbException ex)
            {
                Log.Error(ex,"Не удалось выполнить запрос к базе данных на получение роли с id {id}. Данные по соединеню {HttpContext.Connection}", id, HttpContext.Connection);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CpcmRoleName,CpcmCanEditUsers,CpcmCanEditGroups,CpcmCanEditRoles,CpcmCanDelUsersPosts,CpcmCanDelUsersComments,CpcmCanDelGroupsPosts,CpcmCanAddPost,CpcmCanAddGroups,CpcmCanAddComments")] CpcmRole cpcmRole)
        {
            Log.Information ("Пользователь {HttpContext.User.Identity.Name} пытается изменить роль {cpcmRole}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
            if (id != cpcmRole.CpcmRoleId)
            {
                Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить id роли. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, HttpContext.Connection);
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена роль с таким id";
				return View("UserError");
			}

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cpcmRole);
                    await _context.SaveChangesAsync();
                    Log.Information("Пользователю {HttpContext.User.Identity.Name} удалось изменить роль {cpcmRole}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CpcmRoleExists(cpcmRole.CpcmRoleId))
                    {
                        Log.Error(ex,"Пользователь {HttpContext.User.Identity.Name} попытался изменить роль с id {id}, сохранение не удалось произвести. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, id, HttpContext.Connection);
                        return NotFound();
                    }
                    else
                    {
                        Log.Error(ex,"Не удалось выполнить запрос к базе данных на изменение роли {cpcmRole}. Данные по соединеню {HttpContext.Connection}", cpcmRole, HttpContext.Connection);
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Ошибка связи с сервером";
						return View("UserError");
					}
                }
                return RedirectToAction(nameof(Index));
            }
            Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить роль с некорректными данными {data}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, cpcmRole, HttpContext.Connection);
            return View(cpcmRole);
        }

        //// GET: Roles/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var cpcmRole = await _context.CpcmRoles
        //        .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
        //    if (cpcmRole == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(cpcmRole);
        //}

        //// POST: Roles/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var cpcmRole = await _context.CpcmRoles.FindAsync(id);
        //    if (cpcmRole != null)
        //    {
        //        _context.CpcmRoles.Remove(cpcmRole);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost]
        public async Task<IActionResult> EditUserRole()
        {
            Log.Information("Пользователь {HttpContext.User.Identity.Name} пытается изменить роль пользователя. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, HttpContext.Connection);
            try
            {
                ViewData["CpcmRoles"] = new SelectList(await _context.CpcmRoles.ToListAsync(), "CpcmRoleId", "CpcmRoleName");
                return View();
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на получение списка ролей");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        [HttpPost]
		public async Task<IActionResult> EditUserRole(Guid userId, int roleId)
		{
            Log.Information("Пользователь {HttpContext.User.Identity.Name} пытается изменить роль пользователя {userId} на {roleId}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, userId,roleId, HttpContext.Connection);
            try
            {
                var user = await _context.CpcmUsers.Where(u => u.CpcmUserId == userId).FirstOrDefaultAsync();
                if(user == null)
                {
                    Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить роль пользователя {userId} на {roleId}, но такого пользователя не существует. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, userId, roleId, HttpContext.Connection);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пользователь с данным id не найден";
					return View("UserError");
				}
                var role = await _context.CpcmGroupRoles.Where(r=>r.CpcmRoleId==roleId).FirstOrDefaultAsync();
                if ((role==null))
                {
                    Log.Warning("Пользователь {HttpContext.User.Identity.Name} попытался изменить роль пользователя {userId} на {roleId}, но такой роли не существует. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, userId, roleId, HttpContext.Connection);
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Роль не найдена";
					return View("UserError");
				}
                user.CpcmUserRole = roleId;
                await _context.SaveChangesAsync();
                Log.Information("Пользователю {HttpContext.User.Identity.Name} удалось изменить роль пользователя {userId} на {roleId}. Данные по соединеню {HttpContext.Connection}", HttpContext.User.Identity.Name, userId, roleId, HttpContext.Connection);
                return StatusCode(200, new { status = true });

			}
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли пользователя {userId} на {roleId}", userId, roleId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
		}

		private bool CpcmRoleExists(int id)
        {
            try
            {
                return _context.CpcmRoles.Any(e => e.CpcmRoleId == id);
            }
            catch (DbException)
            {
                return true;
			}
        }
    }
}
