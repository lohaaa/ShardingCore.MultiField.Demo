using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.MultiField.Demo.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController(ApplicationDbContext dbContext) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetPlayer()
    {
        var firstOrDefaultAsync = await dbContext.Players.Where(x => x.AppCode == "game" && x.GroupCode == "group").FirstOrDefaultAsync();
        return Ok(firstOrDefaultAsync);
    }
}