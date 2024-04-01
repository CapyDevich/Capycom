using System;
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
            try
            {
                return View(await _context.CpcmGroupRoles.ToListAsync());
            }
            catch (DbException)
            {
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
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }

                var cpcmGroupRole = await _context.CpcmGroupRoles
                    .FirstOrDefaultAsync(m => m.CpcmRoleId == id);
                if (cpcmGroupRole == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }

                return View(cpcmGroupRole);
            }
            catch (DbException)
            {
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CpcmRoleName,CpcmCanEditGroup,CpcmCanMakePost,CpcmCanDelPost,CpcmCanEditPost")] CpcmGroupRole cpcmGroupRole)
        {
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
            return View(cpcmGroupRole);
        }

        // GET: GroupRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }

                var cpcmGroupRole = await _context.CpcmGroupRoles.FindAsync(id);
                if (cpcmGroupRole == null)
                {
                    Response.StatusCode = 404;
                    ViewData["ErrorCode"] = 404;
                    ViewData["Message"] = "Роль с данным id не найден";
                    return View("UserError");
                }
                return View(cpcmGroupRole);
            }
            catch (DbException)
            {
				Response.StatusCode = 500;
				ViewData["ErrorCode"] = 500;
				ViewData["Message"] = "Ошибка связи с сервером";
				return View("UserError");
			}
        }

        // POST: GroupRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CpcmRoleName,CpcmCanEditGroup,CpcmCanMakePost,CpcmCanDelPost,CpcmCanEditPost")] CpcmGroupRole cpcmGroupRole)
        {
            if (id != cpcmGroupRole.CpcmRoleId)
            {
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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CpcmGroupRoleExists(cpcmGroupRole.CpcmRoleId))
                    {
						Response.StatusCode = 404;
						ViewData["ErrorCode"] = 404;
						ViewData["Message"] = "Роль с данным id не найден";
						return View("UserError");
					}
                    else
                    {
						Response.StatusCode = 500;
						ViewData["ErrorCode"] = 500;
						ViewData["Message"] = "Ошибка связи с сервером";
						return View("UserError");
					}
                }
                return RedirectToAction(nameof(Index));
            }
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
            catch (DbException)
            {
                return true;
            }
        }
    }
}
