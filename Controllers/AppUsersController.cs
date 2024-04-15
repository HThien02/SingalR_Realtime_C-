using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApplication.Context;
using BlogApplication.Models;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace BlogApplication.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<SignalRServer> _signalRServer;

        public AppUsersController(DatabaseContext context, IHubContext<SignalRServer> signalRServer)
        {
            _context = context;
            _signalRServer = signalRServer;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.AppUsers.ToListAsync());
        }

        public async Task<PartialViewResult> LoadUserInformation()
        {
            try
            {
                int? id = HttpContext.Session.GetInt32("UserID");
                var databaseContext = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserID == id);
                return PartialView("PartialViews/AppUsers/_UserInformationPartial", databaseContext);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PartialViewResult> LoadUsers()
        {
            try
            {
                int? id = HttpContext.Session.GetInt32("UserID");
                var databaseContext = await _context.AppUsers.Where(u => u.UserID != id).ToListAsync();
                return PartialView("PartialViews/AppUsers/_IndexPartial", databaseContext);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // GET: AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AppUsers == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // GET: AppUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,FullName,Address,Email,Password")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }

        // GET: AppUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AppUsers == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }
            return View(appUser);
        }

        // POST: AppUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserID,FullName,Address,Email,Password")] AppUser appUser)
        {
            if (id != appUser.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appUser);
                    await _context.SaveChangesAsync();
                    await _signalRServer.Clients.All.SendAsync("LoadUserPage");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppUserExists(appUser.UserID))
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
            return View(appUser);
        }

        // GET: AppUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AppUsers == null)
            {
                return NotFound();
            }

            var appUser = await _context.AppUsers
                .FirstOrDefaultAsync(m => m.UserID == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // POST: AppUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AppUsers == null)
            {
                return Problem("Entity set 'DatabaseContext.AppUsers'  is null.");
            }
            var appUser = await _context.AppUsers.FindAsync(id);
            if (appUser != null)
            {
                _context.AppUsers.Remove(appUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(int id)
        {
            return _context.AppUsers.Any(e => e.UserID == id);
        }

        public IActionResult LoginPage() => View();

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] Login login)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(p => p.Email.Equals(login.Email) && p.Password.Equals(login.Password));
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("FullName", user.FullName);
            }
            else
            {
                ViewBag.LoginError = "Incorrect Email or Password";
                return View("LoginPage");
            }

            return RedirectToAction("Index", "Posts");
        }

        public IActionResult LogoutPage()
        {
            HttpContext.Session.Remove("UserID");
            HttpContext.Session.Remove("FullName");
            return RedirectToAction(nameof(LoginPage));
        }

        public IActionResult RegisterPage() => View();

        [HttpPost]
        public async Task<IActionResult> Register([Bind("FullName,Address,Email,Password")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appUser);
                await _context.SaveChangesAsync();
                ViewBag.RegisterSuccess = $"Welcome to Blog Application, {appUser.FullName}";
                return View("LoginPage");
            }
            return View("RegisterPage");
        }
    }
}
