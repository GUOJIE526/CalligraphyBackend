using Calligraphy.Models;
using Calligraphy.Services.Interfaces;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Calligraphy.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin,Artist")]
    public class ArtController : Controller
    {
        private readonly ILogger<ArtController> _logger;
        private readonly CalligraphyContext _context;
        private readonly IClientIpService _clientIp;
        private readonly ILogService _log;

        public ArtController(ILogger<ArtController> logger, CalligraphyContext context, IClientIpService clientIp, ILogService log)
        {
            _logger = logger;
            _context = context;
            _clientIp = clientIp;
            _log = log;
        }

        /// <summary>
        /// 上傳圖檔View
        /// </summary>
        /// <returns></returns>
        public IActionResult ArtUpload()
        {
            return View();
        }

        /// <summary>
        /// 作品上傳
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ArtUpload(ArtViewModel model)
        {
            // 檢查檔案是否存在
            if (model.File != null && model.File.Length > 0)
            {
                // 儲存檔案的路徑
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", model.File.FileName);
                // 儲存檔案
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    try
                    {
                        await model.File.CopyToAsync(stream);
                    }
                    catch (Exception ex)
                    {
                        // 處理檔案儲存錯誤
                        ModelState.AddModelError("File", "檔案上傳失敗: " + ex.Message);
                        var user = await _context.TbExhUser.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        await _log.LogAsync(user.UserId, filePath, "檔案上傳失敗", _clientIp.GetClientIP());
                        return View(model);
                    }
                }
            }
            if (string.IsNullOrEmpty(model.Title))
            {
                ModelState.AddModelError("Title", "請輸入作品名稱");
                return View(model);
            }
            //還要檢查日期格式有沒有符合yyyy/MM/dd的格式
            if (!DateTime.TryParse(model.Year, out DateTime createdYear))
            {
                ModelState.AddModelError("Year", "請輸入正確的創作日期格式(yyyy/MM/dd)");
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Year))
            {
                ModelState.AddModelError("Year", "請輸入創作年份");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                // 儲存作品資訊到資料庫
                var art = new TbExhArtwork
                {
                    Title = model.Title,
                    Writer = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), // 使用目前登入的使用者ID
                    Description = model.Content,
                    ImageUrl = model.File != null ? $"/Images/{model.File.FileName}" : null,
                    CreatedYear = DateTime.Parse(model.Year),
                    Style = model.Style,
                    Material = model.Material,
                    Dimensions = model.Size,
                    IsVisible = model.IsVisible,
                };
                _context.TbExhArtwork.Add(art);
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = true;
                    return RedirectToAction("ArtUpload");
                }
                catch (Exception ex)
                {
                    // 處理資料庫儲存錯誤
                    _logger.LogError(ex, "資料庫儲存失敗: {Message}", ex.Message);
                    await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "ArtController.ArtUpload", ex.Message, _clientIp.GetClientIP());
                    return View("ArtUpload");
                }
            }
            _logger.LogError("驗證失敗");
            return View(model);
        }

        /// <summary>
        /// 回傳作品圖片
        /// </summary>
        /// <param name="artWorkId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArtWorkImages([FromBody] Guid artWorkId)
        {
            if (artWorkId == Guid.Empty)
            {
                return NotFound();
            }
            var artImage = await _context.TbExhArtwork
                .AsNoTracking()
                .Where(a => a.ArtworkId == artWorkId)
                .Select(a => new
                {
                    a.ImageUrl,
                }).FirstOrDefaultAsync();
            if (artImage == null)
            {
                return NotFound("找不到圖片");
            }
            return Json(new { success = true, artImage = artImage.ImageUrl });
        }

    }
}
