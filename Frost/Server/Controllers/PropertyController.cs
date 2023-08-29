using BrunoZell.ModelBinding;
using Frost.Server.EntityModels;
using Frost.Server.Repositories;
using Frost.Server.Services.ImageServices;
using Frost.Server.Services.Interfaces;
using Frost.Shared.Models;
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
        private IPropertyRepository propertyDbService;
        private IAuthService authService;
        private IPropertyImageService propertyImageService;
        private IUserRepository userDbService;
        public PropertyController(IPropertyRepository propertyService, IAuthService authService, IUserRepository userDbService, IPropertyImageService propertyImageService)
        {
            this.propertyDbService = propertyService;
            this.authService = authService;
            this.userDbService = userDbService;
            this.propertyImageService = propertyImageService;
        }

        [HttpGet("promotedOffers")]
        public async Task<IActionResult> GetPromotedOffers()
        {
            var properties = await propertyDbService.GetPromotedPropertiesAsync();
            return Ok(properties);
        }
        [HttpGet("details")]
        public async Task<IActionResult> GetPropertyDetails(int offerId)
        {
            PropertyDetailsDTO propertyDetails = await propertyDbService.GetPropertyDetailsAsync(offerId);
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
                var properties = await propertyDbService.GetUserPropertiesAsync(usermail);
                return Ok(properties);
            }
            else
            {
                return Unauthorized();
            }

        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProperty([ModelBinder(BinderType = typeof(JsonModelBinder))] PropertyFormModel newProperty, [FromForm] IEnumerable<IFormFile> files)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string usermail = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;

            FileStatusCode fileStatus;
            bool success;
            int propertyId;
            var minRequiredFiles = 6;
            var maxAllowedFiles = 15;
            long maxFileSize = 1024 * 1024 * 5;

            fileStatus = propertyImageService.ValidateImages(files, minRequiredFiles, maxAllowedFiles, maxFileSize);
            if (fileStatus != FileStatusCode.Success)
                return BadRequest(fileStatus.ToString());

            (success, propertyId) = await propertyDbService.CreatePropertyAsync(newProperty, usermail);
            if (success)
            {
                await propertyImageService.SaveImages(files, propertyId.ToString());
                return Ok(propertyId);
            }
            return StatusCode(500);
        }
        [Authorize]
        [HttpPut("{propertyId}")]
        public async Task<IActionResult> UpdateProperty([ModelBinder(BinderType = typeof(JsonModelBinder))] PropertyFormModel newProperty, [FromForm] IEnumerable<IFormFile> files, int propertyId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string usermail = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            bool handleFiles = files.Count() > 0;
            if (handleFiles)
            {
                FileStatusCode fileStatus;
                var minRequiredFiles = 6;
                var maxAllowedFiles = 15;
                long maxFileSize = 1024 * 1024 * 5;

                fileStatus = propertyImageService.ValidateImages(files, minRequiredFiles, maxAllowedFiles, maxFileSize);
                if (fileStatus != FileStatusCode.Success)
                    return BadRequest(fileStatus.ToString());
            }
            bool success;
            PropertyDetailsDTO property;
            (success,property)= await propertyDbService.UpdatePropertyAsync(newProperty, usermail, propertyId);
            if (success)
            {
                if (handleFiles)
                    await propertyImageService.SaveImages(files, propertyId.ToString());
                return Ok(property);
            }
            return StatusCode(500);
        }

        [Authorize]
        [HttpDelete("{propertyId}")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            bool success = await propertyDbService.DeletePropertyAsync(propertyId, int.Parse(userId));
            if (success)
                return Ok();
            return StatusCode(500);
        }
        [Authorize]
        [HttpGet("{propertyId}")]
        public async Task<IActionResult> GetPropertyToEdit(int propertyId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            PropertyFormModel? property = await propertyDbService.GetPropertyToEditAsync(propertyId, int.Parse(userId));
            if (property is not null)
                return Ok(property);
            return StatusCode(404);
        }
        [Authorize]
        [HttpPost("FollowOffer")]
        public async Task<IActionResult> FollowOffer(UserOfferModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != model.userId)
                return Unauthorized();
            PropertyStatusCode status = await propertyDbService.FollowOfferAsync(model.offerId, model.userId);
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
                case PropertyStatusCode.PropertyIsAlreadyFollowed:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [Authorize]
        [HttpPost("UnFollowOffer")]
        public async Task<IActionResult> UnFollowOffer(UserOfferModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != model.userId)
                return Unauthorized();
            PropertyStatusCode status = await propertyDbService.UnfollowOfferAsync(model.offerId, model.userId);
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
                case PropertyStatusCode.PropertyIsNotFollowed:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [HttpGet("FollowedOffers/{userId}")]
        public async Task<IActionResult> GetFollowedOffers(int userId)
        {
            PropertyStatusCode status;
            IEnumerable<PropertyDetailsDTO> followedOffers;
            (status, followedOffers) = await propertyDbService.GetFollowedOffersAsync(userId);
            switch (status)
            {
                case PropertyStatusCode.Success:
                    {
                        return Ok(followedOffers);
                    }
                case PropertyStatusCode.UserDoesNotExists:
                    {
                        return NotFound(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [HttpGet("IsFollowed/{userId}/{offerId}")]
        public async Task<IActionResult> IsPropertyFollowed(int userId, int offerId)
        {
            bool isFollowed = await propertyDbService.isOfferFollowedByUserAsync(offerId, userId);
            return Ok(isFollowed);
        }
        [HttpGet("IsUserProperty/{userId}/{offerId}")]
        public async Task<IActionResult> IsUserProperty(int userId, int offerId)
        {
            bool isUserProperty = await propertyDbService.isUserPropertyAsync(offerId, userId);
            return Ok(isUserProperty);
        }
        [Authorize]
        [HttpPost("{offerId}/Roommates")]
        public async Task<IActionResult> JoinRoommates(UserOfferModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != model.userId)
                return Unauthorized();
            PropertyStatusCode status = await propertyDbService.AddUserToRoommates(model.offerId, model.userId);
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
                case PropertyStatusCode.UserIsAlreadyRoommate:
                    {
                        return Conflict(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotAllowRoommates:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [Authorize]
        [HttpDelete("{offerId}/Roommates/{userId}")]
        public async Task<IActionResult> LeaveRoommates(int offerId, int userId)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string userIdFromToken = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (int.Parse(userIdFromToken) != userId)
                return Unauthorized();
            PropertyStatusCode status = await propertyDbService.RemoveUserFromRoommates(offerId, userId);
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
                case PropertyStatusCode.UserIsNotRoommate:
                    {
                        return Conflict(status.ToString());
                    }
                case PropertyStatusCode.PropertyDoesNotAllowRoommates:
                    {
                        return Conflict(status.ToString());
                    }
                default:
                    return StatusCode(500, status.ToString());
            }
        }
        [HttpGet("search/{propertyType}/{offerType}")]
        public async Task<IActionResult> FilterProperties(
            string propertyType,
            string offerType,
            [FromQuery] string? marketType,
            [FromQuery] string? cityId,
            [FromQuery] int? minRoomsNumber,
            [FromQuery] int? maxRoomsNumber,
            [FromQuery] int? minFloor,
            [FromQuery] int? maxFloor,
            [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice,
            [FromQuery] double? minMeterPrice,
            [FromQuery] double? maxMeterPrice,
            [FromQuery] float? minSurface,
            [FromQuery] float? maxSurface,
            [FromQuery] int? minConstructionYear,
            [FromQuery] int? maxConstructionYear,
            [FromQuery] bool? allowRoommates,
            [FromQuery] string? constructionType,
            [FromQuery] int pageNumber = 1,
            [FromQuery] string? sortBy = "DateDesc"
            )
        {
            IEnumerable<PropertyDetailsDTO> filteredProperties;
            int remainingRecords;
            (filteredProperties, remainingRecords) = await propertyDbService.SearchForOffersAsync(propertyType, offerType, marketType, cityId, minRoomsNumber, maxRoomsNumber, minFloor, maxFloor, minPrice, maxPrice, minMeterPrice, maxMeterPrice, minSurface, maxSurface, minConstructionYear, maxConstructionYear, allowRoommates, constructionType, (pageNumber - 1) * 10,sortBy);
            SearchResult result = new SearchResult() {
                searchedOffers = filteredProperties,
                remainingResults = remainingRecords
            };
            return Ok(result);
        }
        [Authorize]
        [HttpPost("{offerId}/viewcount")]
        public async Task<IActionResult> IncrementPropertyViewCount(UserOfferModel model, int offerId)
        {
            if (model.offerId != offerId)
                return BadRequest();
            await propertyDbService.AddViewToPropertyAsync(model.offerId, model.userId);
            return Ok();
        }
        [HttpGet("{offerId}/viewcount")]
        public async Task<IActionResult> GetPropertyViewCount(int offerId)
        {
            int viewCount = await propertyDbService.GetPropertyViewsCountAsync(offerId);
            return Ok(viewCount);
        }
        [HttpGet("{offerId}/followcount")]
        public async Task<IActionResult> GetPropertyFollowCount(int offerId)
        {
            int followCount = await propertyDbService.GetPropertyFollowCountAsync(offerId);
            return Ok(followCount);
        }


    }

}
