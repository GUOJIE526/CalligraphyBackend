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

        //刪除該筆圖片
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArtWork(Guid Id)
        {
            var artwork = await _context.TbExhArtwork
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArtworkId == Id);
            if (artwork == null)
            {
                return NotFound();
            }
            var likes = await _context.TbExhLike
                .AsNoTracking()
                .Where(l => l.ArtworkId == Id)
                .ToListAsync();
            if (likes.Any())
            {
                _context.TbExhLike.RemoveRange(likes);
            }
            _context.TbExhArtwork.Remove(artwork);
            try
            {
                await _context.SaveChangesAsync();
                await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "刪除圖片", $"{User.Identity.Name} 刪除圖片ID: {Id}", _clientIp.GetClientIP());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting artwork with ID {Id}", Id);
                return Json(new { success = false, message = "刪除失敗" });
            }
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

        /// <summary>
        /// 按下加line好友要先存作者ID進TbExhLine
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddLineFriend([FromBody] Guid Id)
        //{
        //    if (Id == Guid.Empty)
        //    {
        //        return NotFound();
        //    }
        //    var existingLineFriend = await _context.TbExhLine
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(l => l.UserId == Id);
        //    //先刪除已存在的Line好友資料，避免重複新增
        //    if (existingLineFriend != null)
        //    {
        //        _context.TbExhLine.Remove(existingLineFriend);
        //        await _context.SaveChangesAsync();
        //    }
        //    var lineFriend = new TbExhLine
        //    {
        //        UserId = Id
        //    };
        //    _context.TbExhLine.Add(lineFriend);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        await _log.LogAsync(Id, "新增Line好友", $"新增Line好友ID: {Id}", _clientIp.GetClientIP());
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding Line friend with ID {Id}", Id);
        //        return Json(new { success = false, message = "新增失敗" });
        //    }
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
