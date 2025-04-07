using ChatApp.API.Data;
using ChatApp.API.Data.DTOs.UserDTO;
using ChatApp.API.Services;
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
    [Authorize]
    [ApiController]
    public class FriendsController : ControllerBase
    {
        public UserManager<ApplicationUser> _userManager;
        public ApplicationDbContext _context;
        public FriendsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [HttpPut("add")]
        public async Task<FormResult> AddFriendsAsync([FromBody] string newFriendId)
        {
            if (string.IsNullOrEmpty(newFriendId))
            {
                return new FormResult() { Succeeded = false, Errors = ["No friends specified"] };
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
                if (userId is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not authorized"] };
                }
                var user = await _userManager.Users.AsNoTracking().Include(u => u.Friends).ThenInclude(f => f.Friend).FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not authorized"] };

                }
                var friendExists = await _context.Relationships.AsNoTracking().AnyAsync(r => (r.UserId == userId && r.FriendId == newFriendId) || (r.FriendId == userId && r.UserId == newFriendId));
                if (friendExists)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are already friends with this user"] };

                }



                if (newFriendId == userId)
                {

                    return new FormResult() { Succeeded = false, Errors = ["You cant add yourself as a friend"] };
                       
                }


                ApplicationUser newFriend = (await _userManager.FindByIdAsync(newFriendId))!;
                if (newFriend is null)
                {
                    return new FormResult() { Succeeded = false, Errors = [$" A member not found {newFriendId}"] };

                }
                if(_context.FriendRequests.Any(u => (u.SenderId == userId && u.ReceiverId == newFriendId) || ((u.ReceiverId == userId && u.SenderId == newFriendId))))
                {
                    throw new Exception("Friend Request already exists");
                }
                var friendRequest = new FriendRequest() { SenderId = userId, ReceiverId = newFriendId, RequestDate = DateTime.Now };

                var sendFriendRequest = await _context.FriendRequests.AddAsync(friendRequest);

                Console.WriteLine($"Friend request sent from {userId} to {newFriendId}");



                //updatedFriends.Add(new Relationship
                //{
                //    UserId = userId,
                //    FriendId = newFriendId
                //});
                //updatedFriends.Add(new Relationship
                //{
                //    UserId = newFriendId,
                //    FriendId = userId
                //});



                //if (updatedFriends.Count == 0)
                //{
                //    return new FormResult() { Succeeded = false, Errors = ["No valid users to add"] };

                //}
                //_context.Relationships.AddRange(updatedFriends);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                //var updatedUser = await _userManager.Users.Include(u => u.Friends).ThenInclude(f => f.Friend).FirstOrDefaultAsync(u => u.Id == userId);
                //if (updatedUser is null)
                //{
                //    return new FormResult() { Succeeded = false, Errors = ["Fatal error!! The user doesnt exist"] };

                //}
                //var result = new UserWithFriendsDTO()
                //{
                //    Id = updatedUser.Id,
                //    UserName = updatedUser.UserName!,
                //    Email = updatedUser.Email!,
                //    ImageUrl = updatedUser.ImageUrl,
                //    Friends = updatedUser.Friends.Select(r => new BaseApplicationUserDTO
                //    {
                //        Id = r.FriendId,
                //        UserName = r.Friend.UserName!,
                //        Email = r.Friend.Email!,
                //        ImageUrl = r.Friend.ImageUrl
                //    }).ToList()

                //};
                return new FormResult() { Succeeded = true, Errors = null};

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };

            }
        }

        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFriendsAsync(string userId)
        {
            //var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();

            var friends = await _context.Relationships
                .AsNoTracking()
                .Include(f => f.Friend)
                .Where(u => u.UserId == userId)
                .Select(f => f.Friend).ToListAsync();


            var result = friends.Select(f =>
            {
                return new BaseApplicationUserDTO
                {
                    Id = f.Id,
                    Email = f.Email!,
                    UserName = f.UserName!,
                    ImageUrl = f.ImageUrl
                };
            });

            return Ok(result);
        }

        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetFriendsCountAsync(string userId)
        {
            //var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();

            var result = await _context.Relationships
                .AsNoTracking()
                .Include(f => f.Friend)
                .Where(u => u.UserId == userId)
                .CountAsync() ;


            return Ok(result);
        }

        [HttpGet("add/{userId}")]
        public async Task<IActionResult> GetNonFriendsAsync(string userId)
        {
          //  var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();

            var nonfriends = await _context.Users.AsNoTracking()
                .Where(u => u.Id != userId && !_context.Relationships.Any(f => f.UserId == userId && f.FriendId == u.Id))
                .Select(f => new BaseApplicationUserDTO
                {
                    Id = f.Id,
                    Email = f.Email!,
                    UserName = f.UserName!,
                    ImageUrl = f.ImageUrl
                }).ToListAsync();

            return Ok(nonfriends);
        }

        [HttpDelete("{friendId}")]
        public async Task<FormResult> UnfriendAsync(string friendId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.Where(a => a.Type == ClaimTypes.NameIdentifier).Select(a => a.Value).FirstOrDefault();
                var friends = await _context.Relationships.Where(r => (r.UserId == userId && r.FriendId == friendId) || (r.UserId == friendId && r.FriendId == userId)).ToListAsync();
                if (friends.Count == 0)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not friends with this user"] };
                }
                _context.Relationships.RemoveRange(friends);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new FormResult() { Succeeded = true, Errors = null };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
            }
        }
    }
}
