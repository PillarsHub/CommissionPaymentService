using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BatchesController : ControllerBase
    {
        private readonly BatchQueue _batchQueue;

        public BatchesController(BatchQueue batchQueue)
        {
            _batchQueue = batchQueue;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody] Batch batch)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pqEnv = HttpContext.Request.Headers["x-pq_environment"].ToString();

                var workItem = new BatchWorkItem
                {
                    Batch = batch,
                    CallbackToken = HttpContext.Request.Headers["x-callbacktoken"].ToString(),
                    CallbackTokenExpiration = HttpContext.Request.Headers["x-callbackexpire"].ToString(),
                    ClientId = HttpContext.Request.Headers["x-pq_clientId"].ToString(),
                    ClientSecret = HttpContext.Request.Headers["x-pq_clientSecret"].ToString(),
                    FundingAccountPublicId = HttpContext.Request.Headers["x-pq_fundingAccountPublicId"].ToString(),
                    Environment = pqEnv == "s" ? PaymentEnvironment.Sandbox : PaymentEnvironment.Live
                };

                _batchQueue.Enqueue(workItem);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
