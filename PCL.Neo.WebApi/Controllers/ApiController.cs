using Microsoft.AspNetCore.Mvc;
using PCL.Neo.WebApi.Models;
using PCL.Neo.WebApi.Services;
using System.Threading.Tasks;

namespace PCL.Neo.WebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController(IDoSomethingService doSomethingService) : ControllerBase
    {
        private readonly IDoSomethingService _doSomethingService = doSomethingService;

        [HttpPost("do-something")]
        public IActionResult DoSomething([FromBody] MyPayload payload)
        {
            _doSomethingService.DoSomething(payload.module, payload.message);
            return Ok(new { success = true });
        }
    }
}
