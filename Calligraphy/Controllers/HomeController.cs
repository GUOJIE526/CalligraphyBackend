using Calligraphy.Models;
using Calligraphy.Services.Interfaces;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Diagnostics;
using System.Security.Claims;

namespace Calligraphy.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin,Artist")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CalligraphyContext _context;
        private readonly IClientIpService _clientIp;
        private readonly ILogService _log;
        
        public HomeController(ILogger<HomeController> logger, CalligraphyContext context, IClientIpService clientIp, ILogService log)
        {
            _logger = logger;
            _context = context;
            _clientIp = clientIp;
            _log = log;
        }

        public IActionResult Index()
        {
            var newComment = _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .Where(c => !string.IsNullOrEmpty(c.Message) && c.Artwork.Creator == User.FindFirstValue(ClaimTypes.NameIdentifier)).Select(c => new DashboardViewModel
                {
                    dashId = c.CommentId,
                    artTitle = c.Artwork.Title,
                    userName = c.UserName,
                    comment = c.Message!,
                    commentCreate = c.CreateDate,
                    reply = c.Reply
                })
                .OrderByDescending(c => c.commentCreate)
                .ToList();
            return View(newComment);
        }

        /// <summary>
        /// Retrieves a JSON representation of recent comments and their associated artwork details.
        /// </summary>
        /// <remarks>This method queries the database for comments that are associated with artworks and
        /// have a non-empty message. The result includes details such as the comment ID, artwork title, username,
        /// comment message, creation date, and any replies.</remarks>
        /// <returns>A JSON-formatted <see cref="IActionResult"/> containing a list of comments and their associated artwork
        /// details. Each item in the list is represented as a <see cref="DashboardViewModel"/>.</returns>
        public IActionResult DashboardJson()
        {
            //TbExhArtwork, TbExhComment關聯帶出List
            var newComment = _context.TbExhComment
                .AsNoTracking()
                .Include(c => c.Artwork)
                .Where(c => !string.IsNullOrEmpty(c.Message) && c.Artwork.Creator == User.FindFirstValue(ClaimTypes.NameIdentifier)).Select(c => new DashboardViewModel
                {
                    dashId = c.CommentId,
                    artWorkId = c.Artwork.ArtworkId,
                    artTitle = c.Artwork.Title,
                    userName = c.UserName,
                    comment = c.Message!,
                    commentCreate = c.CreateDate,
                    reply = c.Reply
                })
                .OrderByDescending(c => c.commentCreate)
                .ToList();
            return Json(newComment);
        }

        /// <summary>
        /// Reply視窗檢視
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reply視窗回覆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
                newComment.Reply = model.reply;
                try
                {
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

        public IActionResult AllArtWork()
        {
            return View();
        }

       
        //所有已上傳作品
        public IActionResult AllArtworkJson()
        {
            var allArtworks = _context.TbExhArtwork
                .AsNoTracking()
                .Where(a => a.Creator == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(a => new ArtWorkViewModel
                {
                    ArtWorkId = a.ArtworkId,
                    Title = a.Title,
                    Description = a.Description,
                    CreatedYear = a.CreatedYear,
                    Style = a.Style,
                    Material = a.Material,
                    Size = a.Dimensions,
                    IsVisible = a.IsVisible,
                }).ToList();

            return Json(allArtworks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArtWork(ArtWorkViewModel model)
        {
            if (model.ArtWorkId == Guid.Empty)
            {
                return NotFound();
            }
            var artwork = await _context.TbExhArtwork
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtworkId == model.ArtWorkId);
            if (artwork == null)
            {
                return NotFound();
            }
            artwork.Title = model.Title;
            artwork.Description = model.Description;
            artwork.CreatedYear = model.CreatedYear;
            artwork.Style = model.Style;
            artwork.Material = model.Material;
            artwork.Dimensions = model.Size;
            try
            {
                _context.Update(artwork);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating artwork with ID {Id}", model.ArtWorkId);
                return Json(new { success = false, message = "更新失敗" });
            }
            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleIsVisible([FromBody] ToggleVisibilityViewModel model)
        {
            if (model.ArtWorkId == Guid.Empty)
            {
                return NotFound();
            }
            var artwork = await _context.TbExhArtwork
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtworkId == model.ArtWorkId);
            if (artwork == null)
            {
                return NotFound();
            }
            artwork.IsVisible = model.IsVisible;
            try
            {
                _context.Update(artwork);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "隱藏/顯示圖片操作", $"操作異常 {ex.Message}", _clientIp.GetClientIP());
                return Json(new { success = false, message = "系統操作失敗" });
            }
            return Ok();
        }

        /// <summary>
        /// ArtWork視窗檢視
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ArtWorkPartial(Guid Id)
        {
            if (Id == Guid.Empty)
            {
                return NotFound();
            }
            var artwork = await _context.TbExhArtwork
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtworkId == Id);
            if (artwork == null)
            {
                return NotFound();
            }
            var vm = new ArtWorkViewModel
            {
                ArtWorkId = artwork.ArtworkId,
                Title = artwork.Title,
                Description = artwork.Description,
                CreatedYear = artwork.CreatedYear,
                Style = artwork.Style,
                Material = artwork.Material,
                Size = artwork.Dimensions,
                IsVisible = artwork.IsVisible
            };
            return PartialView("_ArtWorkPartial", vm);
        }

        //按讚紀錄
        public IActionResult LikeRecord()
        {
            return View();
        }
        //按讚紀錄Json
        public IActionResult LikeRecordJson()
        {
            //TbExhArtwork, TbExhLike 一對多 一個artId有多個likeId 計算likeId數量
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return NotFound();
            }

            var likeRecord = _context.TbExhLike
                .AsNoTracking()
                .Include(i => i.Artwork)
                .Where(i => i.Artwork.Creator == userId.ToString())
                .GroupBy(i => new { i.ArtworkId, i.Artwork.Title })
                .Select(g => new DashboardViewModel
                {
                    likeId = g.Select(i => i.LikeId).FirstOrDefault(),
                    artId = g.Key.ArtworkId,
                    artTitle = g.Key.Title,
                    likeCount = g.Count()
                })
                .OrderByDescending(x => x.likeCount)
                .ToList();

            return Json(likeRecord);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
