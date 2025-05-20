using System.Diagnostics;
using Calligraphy.Models;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace Calligraphy.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookie, ArtistCookie", Roles = "Admin,Artist")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CalligraphyContext _context;
        public HomeController(ILogger<HomeController> logger, CalligraphyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var newComment = _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .Where(c => !string.IsNullOrEmpty(c.Message)).Select(c => new DashboardViewModel
                {
                    dashId = c.CommentId,
                    artTitle = c.Artwork.Title,
                    userName = c.UserName,
                    comment = c.Message!,
                    commentCreate = c.CreateDate,
                    reply = c.Reply
                }).ToList();
            return View(newComment);
        }

        public IActionResult DashboardJson()
        {
            //TbExhArtwork, TbExhComment關聯帶出List
            var newComment = _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .Where(c => !string.IsNullOrEmpty(c.Message)).Select(c => new DashboardViewModel
                {
                    dashId = c.CommentId,
                    artTitle = c.Artwork.Title,
                    userName = c.UserName,
                    comment = c.Message!,
                    commentCreate = c.CreateDate,
                    reply = c.Reply
                }).ToList();
            return Json(newComment);
        }

        public async Task<IActionResult> ReplyPartial(Guid Id)
        {
            if(Id == Guid.Empty)
            {
                return NotFound();
            }
            var comment = await _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .FirstOrDefaultAsync(c => c.CommentId == Id);
            if(comment == null)
            {
                return NotFound();
            }
            var vm = new DashboardViewModel
            {
                dashId = comment!.CommentId,
                artTitle = comment.Artwork.Title,
                userName = comment.UserName,
                comment = comment.Message!,
                reply = comment.Reply
            };
            return PartialView("_ReplyPartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(DashboardViewModel model)
        {
            if (model.dashId == Guid.Empty)
            {
                return NotFound();
            }
            var newComment = await _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .FirstOrDefaultAsync(c => c.CommentId == model.dashId);
            if (newComment != null)
            {
                try
                {
                    newComment.Reply = model.reply;
                    _context.Update(newComment);
                    await _context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error updating comment with ID {Id}", model.dashId);
                    return Json(new { success = false, message = "回覆失敗" });
                }
            }
            return Json(new { success = true });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
