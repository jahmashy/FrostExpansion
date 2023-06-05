using Frost.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyImagesController : ControllerBase
    {
        private IPropertyImageService propertyImageService;

        public PropertyImagesController(IPropertyImageService propertyImageService)
        {
            this.propertyImageService = propertyImageService;
        }
        [HttpGet("{property_id}/{imageName}")]
        public IActionResult Get(int property_id, string imageName)
        {
            
            Byte[] bytes = System.IO.File.ReadAllBytes($"Resources/PropertyImages/{property_id}/{imageName}");
            return File(bytes,"image/webp");
        }
    }
}
