using Microsoft.AspNetCore.Mvc;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPayQuickerService _payQuickerService;

        public PaymentsController(IPayQuickerService paymentService)
        {
            _payQuickerService = paymentService;
        }

        [HttpGet]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(string userId)
        {
            try
            {
                var balance = await _payQuickerService.GetBalanceForUserAsync(userId);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
