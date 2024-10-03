using Microsoft.AspNetCore.Mvc;
using PaymentService.Inerfaces;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BatchesController : ControllerBase
    {
        private readonly IBatchService _batchService;

        public BatchesController(IBatchService batchService)
        {
            _batchService = batchService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] Batch batch)
        {
            try
            {
                string headers = string.Empty;
                foreach (var key in Request.Headers.Keys)
                    headers += key + "=" + Request.Headers[key] + Environment.NewLine;

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _batchService.ProcesseBatch(batch);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
