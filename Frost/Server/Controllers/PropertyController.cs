using Frost.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private IPropertyService propertyService;
        public PropertyController(IPropertyService propertyService) { this.propertyService = propertyService;}

        [HttpGet("promotedOffers")]
        public async Task<IActionResult> Get()
        {
            var properties = await propertyService.GetPromotedPropertiesAsync();
            return Ok(properties);
        }
    }
}
