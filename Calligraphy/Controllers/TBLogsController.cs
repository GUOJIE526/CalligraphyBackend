using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Calligraphy.Models;

namespace Calligraphy.Controllers
{
    public class TBLogsController : Controller
    {
        private readonly CalligraphyContext _context;

        public TBLogsController(CalligraphyContext context)
        {
            _context = context;
        }

        // GET: Logs
        public async Task<IActionResult> Index()
        {
            var logs = await _context.TbExhLog
                .AsNoTracking()
                .Include(g => g.User)
                .Where(g => g.Action == "ArtUpload")
                .OrderByDescending(g => g.CreateDate)
                .ToListAsync();
            return View(logs);
        }

    }
}
