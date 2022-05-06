using BlogApi.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        //[ApiKey]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
