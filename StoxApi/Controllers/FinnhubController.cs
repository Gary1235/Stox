using Microsoft.AspNetCore.Mvc;

namespace StoxApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinnhubController : ControllerBase
    {
        private readonly IFinnhubService _finnhubService;

        public FinnhubController(IFinnhubService finnhubService)
        {
            _finnhubService = finnhubService;
        }
        
        /// <summary>
        /// 取得 即時價格
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        [HttpGet("GetQuoteAsync")]
        public async Task<IActionResult> GetQuoteAsync(string? symbol)
        {
            var quote = await _finnhubService.GetQuoteAsync(symbol);

            return Ok(quote);
        }
    }
}