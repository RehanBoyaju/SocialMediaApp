using ChatApp.API.Data;
using ChatApp.API.Data.DTOs.GroupDTO;
using ChatApp.API.Data.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }



        [HttpPost]
        public async Task<IActionResult> SaveMessageAsync(ChatMessage message)
        {
            if (message.FromUserId == null)
            {
                var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
                message.FromUserId = userId;
            }
            message.CreatedDate = DateTime.Now;
            if (message.ToUserId != null)
            {
                message.ToUser = await _context.Users.Where(user => user.Id == message.ToUserId).FirstOrDefaultAsync();
            }
            else if (message.ToGroupId != null)
            {
                message.ToGroup = await _context.Groups.Where(group => group.Id == message.ToGroupId).FirstOrDefaultAsync();
            }
            await _context.ChatMessages.AddAsync(message);
            return Ok(await _context.SaveChangesAsync());
        }



        [HttpGet("{contactId}")]
        public async Task<IActionResult> GetConversationAsync(string contactId)
        {
            var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            var messages = await _context.ChatMessages
                    .Where(h => (h.FromUserId == contactId && h.ToUserId == userId) || (h.FromUserId == userId && h.ToUserId == contactId))
                    .OrderBy(a => a.CreatedDate)
                    .Include(a => a.FromUser)
                    .Include(a => a.ToUser)
                    .Select(x => new ChatMessage
                    {
                        FromUserId = x.FromUserId,
                        Message = x.Message,
                        CreatedDate = x.CreatedDate,
                        Id = x.Id,
                        ToUserId = x.ToUserId,
                        ToUser = x.ToUser,
                        FromUser = x.FromUser
                    }).ToListAsync();
            return Ok(messages);
        }


        [HttpGet("search/group/{groupId}/{searchTerm}")]
        [Authorize]
        public async Task<IActionResult> SearchGroupChatAsync(int groupId, string searchTerm)
        {
            var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            var chats = await _context.ChatMessages
                .Where(h => (h.ToGroupId == groupId && h.Message!.Contains(searchTerm)))
                .OrderBy(a => a.CreatedDate)
                .Include(a => a.FromUser)
                .Include(a => a.ToGroup)
                .ToListAsync();

            return Ok(chats);

        }

        [HttpGet("search/{contactId}/{searchTerm}")]
        [Authorize]
        public async Task<IActionResult> SearchUserChatAsync(string contactId, string searchTerm)
        {
            var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
            var chats = await _context.ChatMessages
                .Where(h => ((h.FromUserId) == userId && (h.ToUserId == contactId) || (h.FromUserId == contactId && h.ToUserId == userId)) && h.Message.Contains(searchTerm))
                .OrderBy(a => a.CreatedDate)
                .Include(a => a.FromUser)
                .Include(a => a.ToUser)
                .ToListAsync();

            return Ok(chats);
            //Dictionary<ChatMessage,List<ChatMessage>> nearbyChats = new();

            //foreach (var chatMessage in chats)
            //{
            //    var baseCreatedDate = chatMessage.CreatedDate;
            //    var low = baseCreatedDate.AddMinutes (-5);
            //    var high = baseCreatedDate.AddMinutes (5);
            //    nearbyChats[chatMessage] = await _context.ChatMessages
            //        .Where( h => ((h.FromUserId) == userId && (h.ToUserId == contactId) || (h.FromUserId == contactId && h.ToUserId == userId) && ((h.CreatedDate >= low && h.CreatedDate <= high) || h.Message.Contains(searchTerm))))
            //        .OrderBy( a=> a.CreatedDate)
            //        .Include(h=>h.FromUser)
            //        .Include(h=> h.ToUser)
            //        .ToListAsync();
            //}

            //return Ok(nearbyChats);
        }
    }
}
