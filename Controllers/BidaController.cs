using BidaCanNoManagement.Models;
using BidaCanNoManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidaCanNoManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidaController : ControllerBase
    {
        private readonly BidaService bidaService;

        public BidaController(BidaService bidaService) 
        {
            this.bidaService = bidaService;
        }

        [HttpPost]
        [Route("update")]
        public IActionResult Update([FromBody] Result result)
        {
            bidaService.Load();
            bidaService.Update(result);
            bidaService.Save();

            return this.Ok(bidaService.Load());
        }

        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok(bidaService.Load());
        }
    }
}
