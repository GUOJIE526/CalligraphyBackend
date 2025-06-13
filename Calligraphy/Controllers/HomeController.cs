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

        //�Ҧ��w�W�ǧ@�~
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
                return Json(new { success = false, message = "��s����" });
            }
            return Json(new { success = true });
        }

        //�R���ӵ��Ϥ�
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
                await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "�R���Ϥ�", $"{User.Identity.Name} �R���Ϥ�ID: {Id}", _clientIp.GetClientIP());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting artwork with ID {Id}", Id);
                return Json(new { success = false, message = "�R������" });
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
                await _log.LogAsync(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), "����/��ܹϤ��ާ@", $"�ާ@���` {ex.Message}", _clientIp.GetClientIP());
                return Json(new { success = false, message = "�t�ξާ@����" });
            }
            return Ok();
        }

        /// <summary>
        /// ArtWork�����˵�
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

        //���g����
        public IActionResult LikeRecord()
        {
            return View();
        }
        //���g����Json
        public IActionResult LikeRecordJson()
        {
            //TbExhArtwork, TbExhLike �@��h �@��artId���h��likeId �p��likeId�ƶq
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
        /// ���U�[line�n�ͭn���s�@��ID�iTbExhLine
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
        //    //���R���w�s�b��Line�n�͸�ơA�קK���Ʒs�W
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
        //        await _log.LogAsync(Id, "�s�WLine�n��", $"�s�WLine�n��ID: {Id}", _clientIp.GetClientIP());
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding Line friend with ID {Id}", Id);
        //        return Json(new { success = false, message = "�s�W����" });
        //    }
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
