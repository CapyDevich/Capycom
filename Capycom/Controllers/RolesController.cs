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
using System.Security.Claims;

namespace Capycom.Controllers
{
	[Authorize(Policy = "CpcmCanEditRoles")]
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
                var roles = await _context.CpcmRoles.ToListAsync();
				//ViewData["CpcmRoles"] = new SelectList(roles, "CpcmRoleId", "CpcmRoleName");
				Log.Information("Пользователь {User} зашел на страницу Roles. Данные по соединеню {@Connection}", HttpContext.User.FindFirstValue("CpcmUserId"),HttpContext.Connection.RemoteIpAddress.ToString()); 
                return View(roles);
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

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Log.Information("Пользователь {User} зашел на страницу Roles/Details и попытался просмотреть несуществующую роль без id. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection.RemoteIpAddress.ToString());
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена роль с таким id";
				return View("UserError");
			}

            try
            {
               Log.Information("Пользователь {User} зашел на страницу Roles/Details и попытался просмотреть роль с id {id}. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), id, HttpContext.Connection.RemoteIpAddress.ToString());
                var cpcmRole = await _context.CpcmRoles
                        .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
                if (cpcmRole == null)
                {
                    Log.Information("Пользователь {User} зашел на страницу Roles/Details и попытался просмотреть роль с id {id}, но такой роли не существует. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), id, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Не найдена роль с таким id";
					return View("UserError");
				}
				return View(cpcmRole);
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

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CpcmRole cpcmRole)
        {
            Log.Information ("Пользователь {User} пытается создать роль {@cpcmRole}. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
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
                    Log.Information("Пользователю {User} удалось создать роль {@cpcmRole}. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
					return RedirectToAction(nameof(Index));
				}
                catch (DbUpdateException ex)
                {
					Log.Error(ex, "Пользователю {User} удалось выполнить запрос к базе данных на добавление роли {@cpcmRole}. Данные по соединению  {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
                catch (DbException ex)
                {
                    Log.Error(ex,"Пользователю {User} удалось выполнить запрос к базе данных на добавление роли {@cpcmRole}. Данные по соединению  {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString()); 
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
                
            }
            Log.Warning("Пользователь {User} попытался создать роль с некорректными данными {@cpcmRole}. Данные по соединеню {@HttpContext.Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
            return View(cpcmRole);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Log.Information("Пользователь {User} пытается изменить роль с id {id}. Данные по соединеню {Connection}", HttpContext.User, id, HttpContext.Connection.RemoteIpAddress.ToString());
            if (id == null)
            {
                Log.Warning("Пользователь {User} попытался изменить роль без указанного id. Данные по соединеню {Connection}", HttpContext.User, HttpContext.Connection.RemoteIpAddress.ToString());
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
                    Log.Warning("Пользователь {User} попытался изменить роль с id {id}, но такой роли не существует. Данные по соединеню {Connection}", HttpContext.User, id, HttpContext.Connection.RemoteIpAddress.ToString());
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Не найдена Роль с таикм id.";
                    return View("UserError");
                }
                Log.Information("Пользователь {User} удалось получить роль с id {id}. Данные по соединеню {Connection}", HttpContext.User, id, HttpContext.Connection.RemoteIpAddress.ToString());
                return View(cpcmRole);
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на получение роли с id {id}. Данные по соединеню {Connection}", id, HttpContext.Connection.RemoteIpAddress.ToString());
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
            catch (DbException ex)
            {
                Log.Error(ex,"Не удалось выполнить запрос к базе данных на получение роли с id {id}. Данные по соединеню {Connection}", id, HttpContext.Connection.RemoteIpAddress.ToString());
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CpcmRole cpcmRole)
        {
            Log.Information ("Пользователь {Name} пытается изменить роль {@cpcmRole}. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
            if (id != cpcmRole.CpcmRoleId)
            {
                Log.Warning("Пользователь {User} попытался изменить id роли. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), HttpContext.Connection.RemoteIpAddress.ToString());
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
                    Log.Information("Пользователю {User} удалось изменить роль {@cpcmRole}. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CpcmRoleExists(cpcmRole.CpcmRoleId))
                    {
                        Log.Error(ex,"Пользователь {User} попытался изменить роль с id {id}, сохранение не удалось произвести, поскольку запись была удалена. Данные по соединеню {Connection}", HttpContext.User.FindFirstValue("CpcmUserId"), id, HttpContext.Connection.RemoteIpAddress.ToString());
						Response.StatusCode = 404;
						ViewData["ErrorCode"] = 404;
						ViewData["Message"] = "Запись была ранее удалена";
						return NotFound();
                    }
                    else
                    {
                        Log.Error(ex,"Не удалось выполнить запрос к базе данных на изменение роли {@cpcmRole}, поскольку кто-то до вас попытался изменить запись. Данные по соединеню {@HttpContext.Connection}", cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
						Response.StatusCode = 409;
						ViewData["ErrorCode"] = 409;
						ViewData["Message"] = "Не удалось изменить запись, т.к. она была кем-то изменена. Повторите попытку, после того, как убедитесь в необходимости изменений.";
						return View("UserError");
					}
                }
                catch (DbUpdateException ex)
                {
					Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли {@cpcmRole}. Данные по соединеню {Connection}", cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Не удалось изменить запись. Неизвестная ошибка"+ex.Message;
					return View("UserError");
				}
                catch (DbException ex)
                {
					Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли {@cpcmRole}. Данные по соединеню {Connection}", cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Не удалось изменить запись. Неизвестная ошибка" + ex.Message;
					return View("UserError");
				}
                return RedirectToAction(nameof(Index));
            }
            Log.Warning("Пользователь {@User} попытался изменить роль с некорректными данными {@data}. Данные по соединеню {onnection}", HttpContext.User.FindFirstValue("CpcmUserId"), cpcmRole, HttpContext.Connection.RemoteIpAddress.ToString());
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

        [NonAction]
        [Obsolete("This method is not used and will be removed in future versions",true)]
        public async Task<IActionResult> EditUserRole()
        {
            Log.Information("Пользователь {@HttpContext.User} пытается изменить роль пользователя. Данные по соединеню {@HttpContext.Connection}", HttpContext.User, HttpContext.Connection);
            try
            {
                ViewData["CpcmRoles"] = new SelectList(await _context.CpcmRoles.ToListAsync(), "CpcmRoleId", "CpcmRoleName");
                return View();
            }
            catch (DbUpdateException ex)
            {
				Log.Error(ex, "Не удалось выполнить запрос к базе данных на получение списка ролей");
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");

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
            Log.Information("Пользователь {User} пытается изменить роль пользователя {userId} на {roleId}. Данные по соединеню {Connection}", HttpContext.Connection.RemoteIpAddress.ToString(), userId,roleId, HttpContext.Connection.RemoteIpAddress.ToString());
            try
            {
                var user = await _context.CpcmUsers.Where(u => u.CpcmUserId == userId).FirstOrDefaultAsync();
                if(user == null)
                {
                    Log.Warning("Пользователь {User} попытался изменить роль пользователя {userId} на {roleId}, но такого пользователя не существует. Данные по соединеню {Connection}", HttpContext.Connection.RemoteIpAddress.ToString(), userId, roleId, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пользователь с данным id не найден";
					return StatusCode(404, new { status = false, message = "Пользователь не найден" });
				}
                var role = await _context.CpcmGroupRoles.Where(r=>r.CpcmRoleId==roleId).FirstOrDefaultAsync();
                if ((role==null))
                {
                    Log.Warning("Пользователь {User} попытался изменить роль пользователя {userId} на {roleId}, но такой роли не существует. Данные по соединеню {Connection}", HttpContext.Connection.RemoteIpAddress.ToString(), userId, roleId, HttpContext.Connection.RemoteIpAddress.ToString());
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Роль не найдена";
					return StatusCode(404, new { status = false, message = "Роль не найдена" });
				}
                user.CpcmUserRole = roleId;
                await _context.SaveChangesAsync();
                Log.Information("Пользователю {User} удалось изменить роль пользователя {userId} на {roleId}. Данные по соединеню {Connection}", HttpContext.Connection.RemoteIpAddress.ToString(), userId, roleId, HttpContext.Connection.RemoteIpAddress.ToString());
                return StatusCode(200, new { status = true });

			}
			catch (DbUpdateConcurrencyException ex)
			{
                Log.Error(ex, "Не удалось изменить запись пользователя {userId}. Ошибка конкуренции", userId);
				ViewData["Message"] = "Не удалось изменить запись пользователя. Ошибка конкуренции";
				return StatusCode(500, new { status = false, message= "Не удалось изменить запись пользователя. Ошибка конкуренции" });
			}
			catch (DbUpdateException ex)
			{
				Log.Fatal(ex, "Не удалось изменить запись пользователя {userId}.", userId);
                ViewData["Message"] = "Не удалось изменить запись пользователя.";
				return StatusCode(500, new { status = false, message= "Не удалось изменить запись пользователя." });
			}
			catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на изменение роли пользователя {userId} на {roleId}", userId, roleId);
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return StatusCode(500, new { status = false, message= "Ошибка связи с сервером" }); ;
			}
		}

		private bool CpcmRoleExists(int id)
        {
            try
            {
                return _context.CpcmRoles.Any(e => e.CpcmRoleId == id);
            }
            catch (DbException ex)
            {
                Log.Error(ex, "Не удалось выполнить запрос к базе данных на проверку существования роли с id {id}", id);
                return true;
			}
        }
    }
}
