using ChatApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FriendRequestController:ControllerBase
    {
        private readonly ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> UserManager;

        public FriendRequestController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
            Context = context;
            UserManager = userManager;
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetFriendRequestsSentAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;

            var result = await Context.FriendRequests.AsNoTracking().Include(f=> f.Sender).Include(f=>f.Receiver).Where(f => f.SenderId == userId && f.IsAccepted == null).ToListAsync();
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetFriendRequestsReceivedAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;

            var result = await Context.FriendRequests.AsNoTracking().Include(f => f.Sender).Include(f => f.Receiver).Where(f => f.ReceiverId == userId && f.IsAccepted == null).ToListAsync();
            return Ok(result);
        }

        [HttpPut("accept")]
        public async Task<FormResult> AcceptFriendRequestAsync([FromBody] string friendId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var acceptReq = await Context.FriendRequests.FirstOrDefaultAsync(f => f.SenderId == friendId && f.ReceiverId == userId && f.IsAccepted == null);
                if (acceptReq is null)
                {
                    return new FormResult() { Succeeded = false, Errors = new[] { "Friend request not found" } };
                }
                acceptReq.IsAccepted = true;
                await Context.SaveChangesAsync();

                var updatedUser = await UserManager.Users.Include(u => u.Friends).ThenInclude(f => f.Friend).FirstOrDefaultAsync(u => u.Id == userId);
                if (updatedUser is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Fatal error!! The user doesnt exist"] };

                }
                var updatedFriends = new List<Relationship>();
                updatedFriends.Add(new Relationship
                {
                    UserId = userId,
                    FriendId = friendId,
                });
                updatedFriends.Add(new Relationship
                {
                    UserId = friendId,
                    FriendId = userId
                });



                if (updatedFriends.Count == 0)
                {
                    return new FormResult() { Succeeded = false, Errors = ["No valid users to add"] };

                }
                Context.Relationships.AddRange(updatedFriends);
                await Context.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine($"Friends added {userId} and {friendId}");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = new[] { ex.Message } };
            }
            return new FormResult() { Succeeded = true, Errors = null };
        }

        [HttpPut("reject")]
        public async Task<FormResult> RejectFriendRequestAsync([FromBody] string friendId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var rejectReq = await Context.FriendRequests.FirstOrDefaultAsync(f => f.SenderId == friendId && f.ReceiverId == userId && f.IsAccepted == null);
                if (rejectReq is null)
                {
                    return new FormResult() { Succeeded = false, Errors = new[] { "Friend request not found" } };
                }
                rejectReq.IsAccepted = true;
                await Context.SaveChangesAsync();
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = new[] { ex.Message } };
            }
            return new FormResult() { Succeeded = true, Errors = null };
        }
    }
}
