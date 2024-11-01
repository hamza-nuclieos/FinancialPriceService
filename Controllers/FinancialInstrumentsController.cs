using Microsoft.AspNetCore.Mvc;
using FinancialPriceService.Models;
using FinancialPriceService.Services;
using System.Collections.Generic;

namespace FinancialPriceService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialInstrumentsController : ControllerBase
    {
        private readonly BinancePriceService _priceService;

        public FinancialInstrumentsController(BinancePriceService priceService)
        {
            _priceService = priceService;
        }

        // Endpoint to get all available instruments
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetInstruments()
        {
            // Return a list of available instruments (you could expand this list as needed)
            return Ok(new List<string> { "BTCUSD" });
        }

        // Endpoint to get the current price of BTCUSD
        [HttpGet("{symbol}/price")]
        public ActionResult<FinancialInstrument> GetInstrumentPrice(string symbol)
        {
            if (symbol.ToUpper() != "BTCUSD")
            {
                return NotFound("Instrument not found.");
            }

            var latestPrice = _priceService.GetLatestPrice();
            if (latestPrice == null)
            {
                return StatusCode(503, "Live price data not available.");
            }

            var instrument = new FinancialInstrument
            {
                Symbol = symbol,
                Price = double.Parse(latestPrice)
            };

            return Ok(instrument);
        }
    }
}
