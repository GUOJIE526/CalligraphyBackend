using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calligraphy.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        //測試抓IP
        [HttpGet("ip")]
        public IActionResult GetIP()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            return Ok(new
            {
                RemoteIp = remoteIp,
                ForwardedFor = forwardedFor
            });
        }

    }
}
