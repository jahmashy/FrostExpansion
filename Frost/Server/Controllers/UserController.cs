using Frost.Server.Models;
using Frost.Server.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Frost.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserDbService userDb;
        public UserController(IUserDbService userDb) { 
            this.userDb = userDb;
        }
        [HttpGet]
        public async Task<IActionResult> GetUser(string email)
        {
            var user =  await userDb.GetUserAsync(email);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
