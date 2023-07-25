using BrunoZell.ModelBinding;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Frost.Shared.Models.Forms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private IPropertyService propertyService;
        private IAuthService authService;
        private IPropertyImageService propertyImageService;
        private IUserDbService userDbService;
        public PropertyController(IPropertyService propertyService, IAuthService authService, IUserDbService userDbService, IPropertyImageService propertyImageService)
        {
            this.propertyService = propertyService;
            this.authService = authService;
            this.userDbService = userDbService;
            this.propertyImageService = propertyImageService;
        }

        [HttpGet("promotedOffers")]
        public async Task<IActionResult> GetPromotedOffers()
        {
            var properties = await propertyService.GetPromotedPropertiesAsync();
            return Ok(properties);
        }
        [HttpGet("details")]
        public async Task<IActionResult> GetPropertyDetails(int offerId)
        {
            PropertyDetailsDTO propertyDetails = await propertyService.GetPropertyDetailsAsync(offerId);
            if (propertyDetails == null)
                return NotFound();
            return Ok(propertyDetails);
        }
        [Authorize]
        [HttpGet("userProperties/{usermail}")]
        public async Task<IActionResult> GetUserProperties(string usermail)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            if (authService.AuthorizeUserIdentityFromEmail(usermail, bearer_token))
            {
                var properties = await propertyService.GetUserPropertiesAsync(usermail);
                return Ok(properties);
            }
            else
            {
                return Unauthorized();
            }
            
        }
        [Authorize]
        [HttpPost]
        public  async Task<IActionResult> CreateProperty([ModelBinder(BinderType = typeof(JsonModelBinder))] PropertyFormModel newProperty, [FromForm] IEnumerable<IFormFile> files)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string usermail = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;

            FileStatusCode fileStatus;
            bool success;
            int propertyId;
            var minRequiredFiles = 5;
            var maxAllowedFiles = 15;
            long maxFileSize = 1024 * 1024 * 5;

            fileStatus = propertyImageService.ValidateImages(files, minRequiredFiles, maxAllowedFiles, maxFileSize);
            if (fileStatus != FileStatusCode.Success)
                return BadRequest(fileStatus.ToString());

            (success, propertyId) = await propertyService.CreateProperty(newProperty, usermail);
            if (success)
            {
                await propertyImageService.SaveImages(files, propertyId.ToString());
                return Ok($"Property with an ID:{propertyId} has been created!");
            }
            return StatusCode(500);
        }
    }
}
