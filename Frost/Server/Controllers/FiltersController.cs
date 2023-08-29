using Frost.Server.EntityModels;
using Frost.Server.Repositories;
using Frost.Shared.Models;
using Frost.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : ControllerBase
    {
        private IFiltersRepository _filtersDbService;
        public FiltersController(IFiltersRepository filtersDbService)
        {
            _filtersDbService = filtersDbService;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserFilters(int userId)
        {
            PropertyFiltersDTO filters = await _filtersDbService.GetUserFiltersAsync(userId);
            return Ok(filters);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUserFilters(FiltersUserModel model)
        {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userIdFromToken = int.Parse(id);

            if (model.userId != userIdFromToken)
                return Unauthorized();
            bool success = await _filtersDbService.CreateUserFiltersAsync(model.filtersDto, model.userId);
            if (success)
                return Ok();
            return StatusCode(500);
        }
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserFilters(int userId) {
            var bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer_token);
            string id = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            int userIdFromToken = int.Parse(id);

            if (userId != userIdFromToken)
                return Unauthorized();
            await _filtersDbService.DeleteUserFilters(userId);
            return Ok();
        }

    }
}
