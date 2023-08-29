using Frost.Server.Repositories;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Frost.Shared.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private IPropertyRepository propertyDbService;
        private IPropertyImageService propertyImageService;
        public TemplateController(IPropertyRepository propertyService,IPropertyImageService propertyImageService)
        {
            this.propertyDbService = propertyService;
            this.propertyImageService = propertyImageService;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserTemplates(int userId)
        {
            List<PropertyDetailsDTO> templates;
            PropertyStatusCode status;

            (status,templates) = await propertyDbService.GetSavedTemplatesAsync(userId);

            switch (status)
            {
                case PropertyStatusCode.Success:
                    {
                        return Ok(templates);
                    }
                case PropertyStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveUserTemplate(UserOfferModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != model.userId)
                return Unauthorized();

            PropertyStatusCode status;
            PropertyDetailsDTO property;
            (status,property)= await propertyDbService.SaveTemplateAsync(model.offerId, model.userId);
            switch (status)
            {
                case PropertyStatusCode.Success:
                    {
                        propertyImageService.CopyImages(model.offerId, property.OfferId);
                        return Ok(property);
                    }
                case PropertyStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.TemplateIsAlreadySaved:
                    {
                        return Conflict(status.ToString());
                    }
                case PropertyStatusCode.UserIsNotOwner:
                    {
                        return Unauthorized(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }

        }
        [Authorize]
        [HttpDelete("{offerId}/{userId}")]
        public async Task<IActionResult> DeleteUserTemplate(int offerId, int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != userId)
                return Unauthorized();
            PropertyStatusCode status = await propertyDbService.DeleteTemplateAsync(offerId, userId);
            switch (status)
            {
                case PropertyStatusCode.Success:
                    {
                        return Ok(status.ToString());
                    }
                case PropertyStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.TemplateDoesNotExist:
                    {
                        return Conflict(status.ToString());
                    }
                case PropertyStatusCode.UserIsNotOwner:
                    {
                        return Unauthorized(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [Authorize]
        [HttpPost("{offerId}")]
        public async Task<IActionResult> PublishFromTemplate(UserOfferModel model, int offerId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != model.userId)
                return Unauthorized();
            if (model.offerId != offerId)
                return BadRequest();
            PropertyStatusCode status;
            PropertyDetailsDTO property;
             (status,property)= await propertyDbService.PublishPropertyFromTemplateAsync(model.offerId, model.userId);
            switch (status)
            {
                case PropertyStatusCode.Success:
                    {
                        propertyImageService.CopyImages(model.offerId, property.OfferId);
                        return Ok(property);
                    }
                case PropertyStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                case PropertyStatusCode.UserIsNotOwner:
                    {
                        return Unauthorized(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
    }
}
