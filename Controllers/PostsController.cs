using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApplication.Context;
using BlogApplication.Models;
using Microsoft.AspNetCore.SignalR;

namespace BlogApplication.Controllers
{
    public class PostsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<SignalRServer> _signalRServer;
        private int PageSize { get; set; }

        public PostsController(DatabaseContext context, IHubContext<SignalRServer> signalRServer)
        {
            _context = context;
            _signalRServer = signalRServer;
            PageSize = Int32.Parse(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["PageSize"]);
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var databaseContext = await _context.Post.Include(p => p.AppUser).Include(p => p.Category).OrderByDescending(p => p.UpdatedDate).ToListAsync();
            var result = databaseContext.Take(PageSize);
            return View(result);
        }

        public PartialViewResult LoadSearchSite()
        {
            return PartialView("PartialViews/Posts/_SearchPartial");
        }

        [HttpPost]
        public async Task<PartialViewResult> LoadPostsBySearchItem([FromBody] SearchItem searchItem)
        {
            if (searchItem.Title == null) searchItem.Title = "";
            if (searchItem.ToDate == DateTime.MinValue) searchItem.ToDate = DateTime.MaxValue;

            var databaseContext = await _context.Post.Include(p => p.AppUser).Include(p => p.Category).OrderByDescending(p => p.UpdatedDate).ToListAsync();
            var result = databaseContext.Where(p => p.Title.ToLower().Contains(searchItem.Title.ToLower()) && p.CreatedDate >= searchItem.FromDate && p.CreatedDate <= searchItem.ToDate);
            return PartialView("PartialViews/Posts/_IndexPartial", result);
        }

        public async Task<PartialViewResult> LoadPagingSite()
        {
            var databaseContext = await _context.Post.Include(p => p.AppUser).Include(p => p.Category).OrderByDescending(p => p.UpdatedDate).ToListAsync();
            return PartialView("PartialViews/Posts/_PagingPartial", databaseContext);
        }

        public async Task<PartialViewResult> LoadPostsByPage(int page)
        {
            var databaseContext = await _context.Post.Include(p => p.AppUser).Include(p => p.Category).OrderByDescending(p => p.UpdatedDate).ToListAsync();
            var result = databaseContext.Skip(PageSize * (page - 1)).Take(PageSize);
            return PartialView("PartialViews/Posts/_IndexPartial", result);
        }

        // GET: Posts/Details/5
        public async Task<PartialViewResult> Details(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return null;
            }

            var post = await _context.Post
                .Include(p => p.AppUser)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PostID == id);

            if (post == null)
            {
                return null;
            }

            return PartialView("PartialViews/Posts/_DetailsPartial", post);
        }

        // GET: Posts/Create
        public PartialViewResult Create()
        {
            ViewBag.UserID = HttpContext.Session.GetInt32("UserID");
            ViewData["AuthorID"] = new SelectList(_context.AppUsers, "UserID", "UserID");
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return PartialView("PartialViews/Posts/_CreatePartial");
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorID,Title,Content,PublishStatus,CategoryID")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedDate = DateTime.Now;
                post.UpdatedDate = DateTime.Now;
                _context.Add(post);
                await _context.SaveChangesAsync();
                await _signalRServer.Clients.All.SendAsync("LoadPostsPage");
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.AppUsers, "UserID", "UserID", post.AuthorID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", post.CategoryID);
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<PartialViewResult> Edit(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return null;
            }

            var post = await _context.Post
                .Include(p => p.AppUser)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PostID == id);

            if (post == null)
            {
                return null;
            }

            //ViewData["AuthorID"] = new SelectList(_context.AppUsers, "UserID", "FullName", post.AuthorID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", post.CategoryID);
            return PartialView("PartialViews/Posts/_EditPartial", post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostID,AuthorID,CreatedDate,UpdatedDate,Title,Content,PublishStatus,CategoryID")] Post post)
        {
            if (id != post.PostID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.UpdatedDate = DateTime.Now;
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                    await _signalRServer.Clients.All.SendAsync("LoadPostsPage");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostID))
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
            ViewData["AuthorID"] = new SelectList(_context.AppUsers, "UserID", "UserID", post.AuthorID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", post.CategoryID);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<PartialViewResult> Delete(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return null;
            }

            var post = await _context.Post
                .Include(p => p.AppUser)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.PostID == id);

            if (post == null)
            {
                return null;
            }

            return PartialView("PartialViews/Posts/_DeletePartial", post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Post == null)
            {
                return Problem("Entity set 'DatabaseContext.Post'  is null.");
            }
            var post = await _context.Post.FindAsync(id);
            if (post != null)
            {
                _context.Post.Remove(post);
            }

            await _context.SaveChangesAsync();
            await _signalRServer.Clients.All.SendAsync("LoadPostsPage");
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.PostID == id);
        }
    }
}
