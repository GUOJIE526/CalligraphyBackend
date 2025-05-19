using System.Diagnostics;
using Calligraphy.Models;
using Calligraphy.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            //TbExhArtwork, TbExhComment關聯帶出List
            var dashboardData = _context.TbExhArtwork
                .AsNoTracking()
                .Where(d => d.IsVisible && d.TbExhComment.Any(c => !string.IsNullOrEmpty(c.Message)))
                .Select(d => new DashboardViewModel
                {
                    ArtTitle = d.Title,
                    Comment = d.TbExhComment.Select(c => c.Message!).ToList(),
                    Reply = d.TbExhComment.Select(c => c.Reply).ToList(),
                }).ToList();
            return View(dashboardData);
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
