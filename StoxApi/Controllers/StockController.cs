using Microsoft.AspNetCore.Mvc;

namespace StoxApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("GetStockList")]
        public IActionResult GetStockList(StockSearchViewModel search)
        {
            var list = _stockService.GetStockList(search);

            return Ok(list);
        }

        [HttpPost("BuyStock")]
        public async Task<IActionResult> BuyStock(TransactionViewModel model)
        {
            var result = await _stockService.BuyStockAsync(model);

            return Ok(result);
        }

        [HttpPost("SellStock")]
        public IActionResult SellStock(TransactionViewModel model)
        {
            var result = _stockService.SellStock(model);

            return Ok(result);
        }
    }
}