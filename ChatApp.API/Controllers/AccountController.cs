using ChatApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            this.userManager = userManager;
            _context = context;
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return BadRequest();
            }
           
            return Ok(currentUser);
        }

        [Authorize]
        [HttpGet("{userId}/ProfileImage")]
        public async Task<IActionResult> ProfileImage(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return File(user.ProfileImage, "image/jpeg");
        }
    }
}
