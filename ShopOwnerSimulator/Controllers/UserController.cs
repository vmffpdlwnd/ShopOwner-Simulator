using Microsoft.AspNetCore.Mvc;
using ShopOwnerSimulator.Models;
using ShopOwnerSimulator.Services;

namespace ShopOwnerSimulator.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly GameService _gameService;
        public UserController(GameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("new")] // POST api/user/new
        public async Task<IActionResult> NewUser([FromBody] string username)
        {
            await _gameService.CreateNewGameAsync(username);
            var user = _gameService.GetCurrentUser();
            if (user == null) return BadRequest();
            return Ok(user);
        }

        [HttpGet("current")] // GET api/user/current
        public IActionResult GetCurrentUser()
        {
            var user = _gameService.GetCurrentUser();
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}
