using Microsoft.AspNetCore.Mvc;
using ShopOwnerSimulator.Services;

namespace ShopOwnerSimulator.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly LambdaService _lambdaService;

        public GameController(GameService gameService, LambdaService lambdaService)
        {
            _gameService = gameService;
            _lambdaService = lambdaService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            var result = await _lambdaService.GetUserInfoAsync(userId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // 추가 API 엔드포인트는 여기에 작성
    }
}
