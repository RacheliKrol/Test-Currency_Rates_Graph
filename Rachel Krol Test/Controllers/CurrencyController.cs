using Microsoft.AspNetCore.Mvc;
using Rachel_Krol_Test.Models;
using Rachel_Krol_Test.Services.Interfaces;

namespace Rachel_Krol_Test.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }


        [HttpGet("rates/week")]
        public async Task<ActionResult<List<CurrencyRate>>> GetRatesForWeek()
        {        
            var start = DateTime.Today.AddDays(-7);
            var rates = await _currencyService.GetRatesByDateRange(start, DateTime.Today);
            return Ok(rates);
        }

        [HttpGet("rates/month")]
        public async Task<ActionResult<List<CurrencyRate>>> GetRatesForMonth()
        {
            var start = DateTime.Today.AddMonths(-1);
            var rates = await _currencyService.GetRatesByDateRange(start, DateTime.Today);
            return Ok(rates);
        }

        [HttpGet("rates/half_year")]
        public async Task<ActionResult<List<CurrencyRate>>> GetRatesForHalfYear()
        {
            var rates = await _currencyService.GetRatesForYear(6);
            return Ok(rates);
        }

        [HttpGet("rates/year")]
        public async Task<ActionResult<List<CurrencyRate>>> GetRatesForYear()
        {
            var rates = await _currencyService.GetRatesForYear(12);
            return Ok(rates);
        }
    }
}
