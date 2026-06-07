using Microsoft.AspNetCore.Mvc;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CapitalFlowController : ControllerBase
    {
        private readonly ICapitalFlowService _flowService;

        public CapitalFlowController(ICapitalFlowService flowService)
        {
            _flowService = flowService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> GetList([FromBody] CapitalFlowSearchViewModel search)
        {
            var result = await _flowService.GetListAsync(search);
            return Ok(result);
        }

        // 存入本金
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(CapitalFlowViewModel model)
        {
            // 假設從 Token 取得目前登入者 ID 作為 CreatedUserId
            var currentUserId = Guid.NewGuid();
            var id = await _flowService.CreateDepositAsync(model);

            return Ok(new { Message = "存入成功", Id = id });
        }

        // 提領本金
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] CapitalFlowViewModel model)
        {
            var currentUserId = Guid.NewGuid();
            var id = await _flowService.CreateWithdrawAsync(model);

            return Ok(new { Message = "提領成功", Id = id });
        }

        // 軟刪除
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _flowService.SoftDeleteAsync(id);

            return Ok(result);
        }

        // 取得總覽
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetSummary(Guid userId)
        {
            var summary = await _flowService.GetSummaryAsync(userId);

            return Ok(summary);
        }
    }
}