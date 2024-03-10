﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Capycom;
using System.Security.Cryptography;

namespace Capycom.Controllers
{
    public class UserSignUpController : Controller
    {
        private readonly CapycomContext _context;

        public UserSignUpController(CapycomContext context)
        {
            _context = context;
        }

        // GET: UserSignUp
        public async Task<IActionResult> Index()
        {
            var capycomContext = _context.CpcmUsers.Include(c => c.CpcmUserCityNavigation).Include(c => c.CpcmUserRoleNavigation).Include(c => c.CpcmUserSchoolNavigation).Include(c => c.CpcmUserUniversityNavigation);
            return View(await capycomContext.ToListAsync());
        }

        // GET: UserSignUp/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpcmUser = await _context.CpcmUsers
                .Include(c => c.CpcmUserCityNavigation)
                .Include(c => c.CpcmUserRoleNavigation)
                .Include(c => c.CpcmUserSchoolNavigation)
                .Include(c => c.CpcmUserUniversityNavigation)
                .FirstOrDefaultAsync(m => m.CpcmUserId == id);
            if (cpcmUser == null)
            {
                return NotFound();
            }

            return View(cpcmUser);
        }

        // GET: UserSignUp/Create
        public IActionResult Create()
        {
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityId");
            ViewData["CpcmUserRole"] = new SelectList(_context.CpcmRoles, "CpcmRoleId", "CpcmRoleId");
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchooldId");
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityId");
            return View();
        }

        // POST: UserSignUp/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CpcmUserId,CpcmUserEmail,CpcmUserTelNum,CpcmUserPwdHash,CpcmUserSalt,CpcmUserAbout,CpcmUserCity,CpcmUserSite,CpcmUserBooks,CpcmUserFilms,CpcmUserMusics,CpcmUserSchool,CpcmUserUniversity,CpcmUserImagePath,CpcmUserCoverPath,CpcmUserNickName,CpcmUserFirstName,CpcmUserSecondName,CpcmUserAdditionalName,CpcmUserRole")] CpcmUser cpcmUser)
        {
            if (ModelState.IsValid)
            {
                cpcmUser.CpcmUserId = Guid.NewGuid();
                cpcmUser.CpcmUserPwdHash = SHA256.HashData(cpcmUser.CpcmUserPwdHash);
                _context.Add(cpcmUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityId", cpcmUser.CpcmUserCity);
            ViewData["CpcmUserRole"] = new SelectList(_context.CpcmRoles, "CpcmRoleId", "CpcmRoleId", cpcmUser.CpcmUserRole);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchooldId", cpcmUser.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityId", cpcmUser.CpcmUserUniversity);
            return View(cpcmUser);
        }

        // GET: UserSignUp/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpcmUser = await _context.CpcmUsers.FindAsync(id);
            if (cpcmUser == null)
            {
                return NotFound();
            }
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityId", cpcmUser.CpcmUserCity);
            ViewData["CpcmUserRole"] = new SelectList(_context.CpcmRoles, "CpcmRoleId", "CpcmRoleId", cpcmUser.CpcmUserRole);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchooldId", cpcmUser.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityId", cpcmUser.CpcmUserUniversity);
            return View(cpcmUser);
        }

        // POST: UserSignUp/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CpcmUserId,CpcmUserEmail,CpcmUserTelNum,CpcmUserPwdHash,CpcmUserSalt,CpcmUserAbout,CpcmUserCity,CpcmUserSite,CpcmUserBooks,CpcmUserFilms,CpcmUserMusics,CpcmUserSchool,CpcmUserUniversity,CpcmUserImagePath,CpcmUserCoverPath,CpcmUserNickName,CpcmUserFirstName,CpcmUserSecondName,CpcmUserAdditionalName,CpcmUserRole")] CpcmUser cpcmUser)
        {
            if (id != cpcmUser.CpcmUserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cpcmUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CpcmUserExists(cpcmUser.CpcmUserId))
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
            ViewData["CpcmUserCity"] = new SelectList(_context.CpcmCities, "CpcmCityId", "CpcmCityId", cpcmUser.CpcmUserCity);
            ViewData["CpcmUserRole"] = new SelectList(_context.CpcmRoles, "CpcmRoleId", "CpcmRoleId", cpcmUser.CpcmUserRole);
            ViewData["CpcmUserSchool"] = new SelectList(_context.CpcmSchools, "CpcmSchooldId", "CpcmSchooldId", cpcmUser.CpcmUserSchool);
            ViewData["CpcmUserUniversity"] = new SelectList(_context.CpcmUniversities, "CpcmUniversityId", "CpcmUniversityId", cpcmUser.CpcmUserUniversity);
            return View(cpcmUser);
        }

        // GET: UserSignUp/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cpcmUser = await _context.CpcmUsers
                .Include(c => c.CpcmUserCityNavigation)
                .Include(c => c.CpcmUserRoleNavigation)
                .Include(c => c.CpcmUserSchoolNavigation)
                .Include(c => c.CpcmUserUniversityNavigation)
                .FirstOrDefaultAsync(m => m.CpcmUserId == id);
            if (cpcmUser == null)
            {
                return NotFound();
            }

            return View(cpcmUser);
        }

        // POST: UserSignUp/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cpcmUser = await _context.CpcmUsers.FindAsync(id);
            if (cpcmUser != null)
            {
                _context.CpcmUsers.Remove(cpcmUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CpcmUserExists(Guid id)
        {
            return _context.CpcmUsers.Any(e => e.CpcmUserId == id);
        }
    }
}
