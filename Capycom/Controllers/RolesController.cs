using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capycom;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;

namespace Capycom.Controllers
{
	[Authorize(Policy = "CanEditRoles")]
	public class RolesController : Controller
    {
        private readonly CapycomContext _context;

        public RolesController(CapycomContext context)
        {
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.CpcmRoles.ToListAsync());
            }
            catch (DbException)
            {
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
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена роль с таким id";
				return View("UserError");
			}

            try
            {
                var cpcmRole = await _context.CpcmRoles
                        .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
                if (cpcmRole == null)
                {
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Не найдена роль с таким id";
					return View("UserError");
				}
				return View(cpcmRole);
			}
            catch (DbException)
            {
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CpcmRoleId,CpcmRoleName,CpcmCanEditUsers,CpcmCanEditGroups,CpcmCanEditRoles,CpcmCanDelUsersPosts,CpcmCanDelUsersComments,CpcmCanDelGroupsPosts,CpcmCanAddPost,CpcmCanAddGroups,CpcmCanAddComments")] CpcmRole cpcmRole)
        {
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
					return RedirectToAction(nameof(Index));
				}
                catch (DbException)
                {
					Response.StatusCode = 500;
					ViewData["ErrorCode"] = 500;
					ViewData["Message"] = "Ошибка связи с сервером";
					return View("UserError");
				}
                
            }
            return View(cpcmRole);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
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
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Не найдена Роль с таикм id.";
                    return View("UserError");
                }
                return View(cpcmRole);
            }
            catch (DbException)
            {
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CpcmRoleId,CpcmRoleName,CpcmCanEditUsers,CpcmCanEditGroups,CpcmCanEditRoles,CpcmCanDelUsersPosts,CpcmCanDelUsersComments,CpcmCanDelGroupsPosts,CpcmCanAddPost,CpcmCanAddGroups,CpcmCanAddComments")] CpcmRole cpcmRole)
        {
            if (id != cpcmRole.CpcmRoleId)
            {
				Response.StatusCode = 404;
				ViewData["ErrorCode"] = 404;
				ViewData["Message"] = "Не найдена рольс с таким id";
				return View("UserError");
			}

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cpcmRole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CpcmRoleExists(cpcmRole.CpcmRoleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
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

        public async Task<IActionResult> EditUserRole()
        {
            try
            {
                ViewData["CpcmRoles"] = new SelectList(await _context.CpcmRoles.ToListAsync(), "CpcmRoleId", "CpcmRoleName");
                return View();
            }
            catch (DbException)
            {
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        [HttpPost]
		public async Task<IActionResult> EditUserRole(Guid userId, int roleId)
		{
            try
            {
                var user = await _context.CpcmUsers.Where(u => u.CpcmUserId == userId).FirstOrDefaultAsync();
                if(user == null)
                {
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Пользователь с данным id не найден";
					return View("UserError");
				}
                var role = await _context.CpcmGroupRoles.Where(r=>r.CpcmRoleId==roleId).FirstOrDefaultAsync();
                if ((role==null))
                {
					Response.StatusCode = 404;
					ViewData["ErrorCode"] = 404;
					ViewData["Message"] = "Роль не найдена";
					return View("UserError");
				}
                user.CpcmUserRole = roleId;
                await _context.SaveChangesAsync();
                return StatusCode(200, new { status = true });

			}
            catch (DbException)
            {
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
