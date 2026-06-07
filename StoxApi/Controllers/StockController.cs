using Microsoft.AspNetCore.Mvc;

namespace StoxApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IPortfolioService _stockService;

        public StockController(IPortfolioService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("GetStockList")]
        public async Task<IActionResult> GetStockList(StockSearchViewModel search)
        {
            var list = await _stockService.GetStockListAsync(search);

            return Ok(list);
        }

        [HttpPost("BuyStock")]
        public async Task<IActionResult> BuyStock(TransactionViewModel model)
        {
            var result = await _stockService.BuyStockAsync(model);

            return Ok(result);
        }

        [HttpPost("SellStock")]
        public async Task<IActionResult> SellStock(TransactionViewModel model)
        {
            var result = await _stockService.SellStockAsync(model);

            return Ok(result);
        }

        [HttpPost("MapCsvToStockMaster")]
        public async Task<IActionResult> MapCsvToStockMaster()
        {
            var result = await _stockService.MapCsvToStockMaster(@"C:\Gary\SideProject\Stox\StoxApi\Data\nasdaq_screener_1778393810671.csv");

            return Ok(result);
        }

        [HttpPost("GetAllStockQuote")]
        public async Task<IActionResult> GetAllStockQuote()
        {
            var quotes = await _stockService.GetAllStockQuoteAsync();

            return Ok(quotes);
        }

        [HttpPost("GetStockOptions")]
        public async Task<IActionResult> GetStockOptions()
        {
            var options = await _stockService.GetStockOptionsAsync();

            return Ok(options);
        }
    }
}