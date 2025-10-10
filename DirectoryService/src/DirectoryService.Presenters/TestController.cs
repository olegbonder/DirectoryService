using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("[controller]")]
    public class TestController: ControllerBase
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok();
        }
    }
}
