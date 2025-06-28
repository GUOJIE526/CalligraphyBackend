using Calligraphy.Models;
using Calligraphy.Services.Interfaces;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
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
        private readonly IConfiguration _config;

        public ArtController(ILogger<ArtController> logger, CalligraphyContext context, IClientIpService clientIp, ILogService log, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _clientIp = clientIp;
            _log = log;
            _config = config;
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

            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(_config["LineBot:ChannelAccessToken"]);
            // 檢查檔案是否存在
            if (model.File != null && model.File.Length > 0)
            {
                var iisStoragePath = _config["ArtStorage:StoragePath"];
                if(!Directory.Exists(iisStoragePath))
                {
                    //新增資料夾
                    Directory.CreateDirectory(iisStoragePath);
                }
                // 儲存檔案的路徑
                var filePath = Path.Combine(iisStoragePath, model.File.FileName);
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
            //還要檢查日期格式有沒有符合yyyy/MM/dd的格式
            if (!DateTime.TryParse(model.Year, out DateTime createdYear))
            {
                ModelState.AddModelError("Year", "請輸入正確的創作日期格式(yyyy/MM/dd)");
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
                //讀取TbExhLine的用戶ID放進List中
                var lineUsers = await _context.TbExhLine
                    .AsNoTracking()
                    .Where(e => e.LineUserId != null && e.Notify == true && e.Unfollow == false)
                    .Select(e => e.LineUserId)
                    .ToListAsync();
                List<string> msg = new List<string>();
                msg.Add($"新作品 {art.Title} 已發布趕緊去看看吧~");

                _context.TbExhArtwork.Add(art);
                try
                {
                    await _context.SaveChangesAsync();
                    await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "ArtUpload", $"{User.Identity.Name} 作品 {art.Title} 上傳成功", _clientIp.GetClientIP());
                    if (lineUsers.Count > 0 && art.IsVisible == true)
                    {
                        bot.PushMulticast(lineUsers, msg);
                    }
                    TempData["SuccessMessage"] = true;
                    return RedirectToAction("ArtUpload");
                }
                catch (Exception ex)
                {
                    // 處理資料庫儲存錯誤
                    _logger.LogError(ex, "資料庫儲存失敗: {Message}", ex.Message);
                    await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "ArtUpload", $"上傳失敗: {ex.Message}", _clientIp.GetClientIP());
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
