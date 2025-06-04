using Microsoft.AspNetCore.Mvc;
using PCL.Neo.Core.Models.Minecraft.Java;
using System.Threading.Tasks;

namespace PCL.Neo.WebApi.Controllers
{
    [ApiController]
    [Route("api/java")]
    public class JavaController(IJavaManager javaManager) : ControllerBase
    {
        private readonly IJavaManager _javaManager = javaManager;

        [HttpGet("list")]
        public async Task<IActionResult> GetJavaList()
        {
            await _javaManager.JavaListInitAsync();
            return Ok(_javaManager.JavaList);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            await _javaManager.Refresh();
            Console.WriteLine("已刷新Java列表");
            return Ok(new { success = true });
        }
    }
}
