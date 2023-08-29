using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyImagesController : ControllerBase
    {
        private IPropertyImageService propertyImageService;
        private IConfiguration _configuration;

        public PropertyImagesController(IPropertyImageService propertyImageService, IConfiguration configuration)
        {
            this.propertyImageService = propertyImageService;
            _configuration = configuration;
        }
        [HttpGet("{property_id}/{imageName}")]
        public IActionResult GetImage(int property_id, string imageName)
        {
            string dirPath = _configuration["Directories:PropertyImages"] + $"{property_id}" + $"/{imageName}";
            Byte[] bytes = System.IO.File.ReadAllBytes(dirPath);
            return File(bytes,"image/webp");
        }
        [HttpGet("{property_id}")]
        public IActionResult GetImages(int property_id)
        {
            List<byte[]> bytesList;
            FileStatusCode statusCode;
            (statusCode, bytesList) = propertyImageService.GetPropertyImages(property_id);
            if(statusCode == FileStatusCode.FileDoesNotExists)
                return NotFound();
            return Ok(bytesList);
        }
    }
}
