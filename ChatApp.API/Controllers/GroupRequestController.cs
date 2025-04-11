using ChatApp.API.Data;
using ChatApp.API.Data.DTOs.GroupDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupRequestController : ControllerBase
    {
        private readonly ApplicationDbContext Context;
        private readonly UserManager<ApplicationUser> UserManager;

        public GroupRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            Context = context;
            UserManager = userManager;
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetGroupRequestsSentAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;

            var result = await Context.GroupRequests.AsNoTracking().Include(f => f.Sender).Include(f => f.Group).Where(f => f.SenderId == userId && f.IsAccepted == null).ToListAsync();
            return Ok(result);
        }


        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupRequestsReceivedAsync(int groupId)
        {
            try
            {
                var result = await Context.GroupRequests.AsNoTracking().Include(f => f.Sender).Include(f => f.Group).Where(f => f.GroupId == groupId && f.IsAccepted == null).ToListAsync();
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPut("accept")]
        public async Task<FormResult> AcceptGroupRequestAsync([FromBody] GroupRequestDTO groupRequest)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var alreadyMember = await Context.GroupMembers.AsNoTracking().AnyAsync(r => r.UserId == groupRequest.senderId && r.GroupId == groupRequest.groupId);
                var IsGroupAdminOrModerator = await Context.GroupMembers.AsNoTracking().AnyAsync(r => (r.UserId == userId && r.GroupId == groupRequest.groupId && (r.IsAdmin || r.IsModerator)));
                if (alreadyMember)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are already friends with this user"] };

                }
                if (!IsGroupAdminOrModerator)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not allowed to accept this request"] };
                }
                var acceptReq = await Context.GroupRequests.FirstOrDefaultAsync(f => f.SenderId == groupRequest.senderId && f.GroupId == groupRequest.groupId && f.IsAccepted == null);
                if (acceptReq is null)
                {
                    return new FormResult() { Succeeded = false, Errors = new[] { "Group request not found" } };
                }
                acceptReq.IsAccepted = true;

                await Context.SaveChangesAsync();





                var updatedGroup = new List<GroupMember>();
                updatedGroup.Add(new GroupMember
                {
                    UserId = groupRequest.senderId,
                    GroupId = groupRequest.groupId
                });




                if (updatedGroup.Count == 0)
                {
                    return new FormResult() { Succeeded = false, Errors = ["No valid members to add"] };

                }
                Context.GroupMembers.AddRange(updatedGroup);
                var grpRequest = await Context.GroupRequests.FirstOrDefaultAsync(f => (f.SenderId == groupRequest.senderId && f.GroupId == groupRequest.groupId));

                if (grpRequest is null)
                {
                    throw new Exception("Group request not found");
                }
                Context.Remove(grpRequest);
                
                await Context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                
                Console.WriteLine($"Members added  {groupRequest.senderId} in {groupRequest.groupId}");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = new[] { ex.Message } };
            }

            return new FormResult() { Succeeded = true, Errors = null };
        }



        [HttpPut("reject")]
        public async Task<FormResult> RejectGroupRequestAsync([FromBody] GroupRequestDTO groupRequest)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var IsGroupAdminOrModerator = await Context.GroupMembers.AsNoTracking().AnyAsync(r => (r.UserId == userId && r.GroupId == groupRequest.groupId && (r.IsAdmin || r.IsModerator)));
                if (!IsGroupAdminOrModerator)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not allowed to reject this request"] };
                }
                var rejectReq = await Context.GroupRequests.FirstOrDefaultAsync(f => f.SenderId == groupRequest.senderId && f.GroupId == groupRequest.groupId && f.IsAccepted == null);
                if (rejectReq is null)
                {
                    return new FormResult() { Succeeded = false, Errors = new[] { "Group request not found" } };
                }
                rejectReq.IsAccepted = false;
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



        [HttpDelete("delete/{groupId}")]
        public async Task<FormResult> DeleteGroupRequestAsync(int groupId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var IsGroupAdminOrModerator = await Context.GroupMembers.AsNoTracking().AnyAsync(r => (r.UserId == userId && r.GroupId == groupId && (r.IsAdmin || r.IsModerator)));
                if (!IsGroupAdminOrModerator)
                {
                    return new FormResult() { Succeeded = false, Errors = ["You are not allowed to delete this user"] };
                }
                var grpRequest = await Context.GroupRequests.FirstOrDefaultAsync(f => (f.SenderId == userId && f.GroupId ==groupId));

                if (grpRequest is null)
                {
                    return new FormResult { Succeeded = false, Errors = new[] { "Group request not found" } };
                }
                Context.Remove(grpRequest);
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
