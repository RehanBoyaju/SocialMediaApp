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
            string mimeType = GetMimeType(user.ProfileImage);
            return File(user.ProfileImage, mimeType);
        }
        private string GetMimeType(byte[] imageData)
        {
            if (imageData.Length > 4)
            {
                // PNG: Starts with 89 50 4E 47 (‰PNG)
                if (imageData[0] == 0x89 && imageData[1] == 0x50 &&
                    imageData[2] == 0x4E && imageData[3] == 0x47)
                {
                    return "image/png";
                }

                // JPEG: Starts with FF D8 FF (ÿØÿ)
                if (imageData[0] == 0xFF && imageData[1] == 0xD8 && imageData[2] == 0xFF)
                {
                    return "image/jpeg";
                }
            }
            return "application/octet-stream"; // Fallback
        }
    }
}
