using ChatApp.API.Data;
using ChatApp.API.Data.DTOs.GroupDTO;
using ChatApp.API.Data.DTOs.UserDTO;
using ChatApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : ControllerBase
    {
        public UserManager<ApplicationUser> UserManager { get; } = userManager;
        public ApplicationDbContext Context { get; } = context;

        [HttpGet]
        public async Task<IActionResult> GetAllGroupsAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
            var user = await Context.Users.AsNoTracking().Where(u => u.Id == userId).FirstOrDefaultAsync();
            var groups = await Context.GroupMembers.AsNoTracking().Where(g => g.UserId == userId).Select(g => g.Group).ToListAsync();

            return Ok(groups);
        }
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupByIdAsync(int groupId)
        {
            
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
            bool isMember = await Context.GroupMembers
                                                             .AsNoTracking()
                                                             .AnyAsync(g => g.UserId == userId && g.GroupId == groupId);

            if (!isMember)
            {
                return BadRequest("Not found");
            }

            // Fetch group details
            //var group = await Context.Groups
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(g => g.Id == groupId);

            //group.Members = await Context.GroupMembers.Include(g => g.User).Where(g => g.GroupId == groupId).ToListAsync();

            var group = await Context.Groups
                                                    .AsNoTracking()
                                                    .Include(g => g.Members)!
                                                        .ThenInclude(m => m.User)
                                                    .FirstOrDefaultAsync(g => g.Id == groupId);

            var groupDto = new BaseGroupDTO
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                ImageUrl = group.ImageUrl,
                // Map members data
                MembersInfo = group.Members.Select(m => new BaseApplicationUserDTO
                {
                    Id = m.User.Id,
                    UserName = m.User.UserName,
                    Email = m.User.Email,
                    ProfileImageUrl= m.User.ProfileImageUrl
                }).ToList(),
                MembersCount = group.Members.Count
            };
            return Ok(groupDto);
        }
        [HttpPost]
        public async Task<IActionResult> CreateGroupAsync(Group createGroup)
        {
            try
            {
                // Create the group first
                //Group group = new()
                //{
                //    Name = createGroup.Name,
                //    Description = createGroup.Description,
                //    MemberIds = createGroup.MemberIds
                //};
                await Context.Groups.AddAsync(createGroup);
                await Context.SaveChangesAsync(); // Ensures group.Id is available

                // Find members
                var invalidMembers = new List<string>();
                var groupMembers = new List<GroupMember>();

                foreach (var memberId in createGroup.MemberIds)
                {
                    var member = await UserManager.FindByIdAsync(memberId);
                    if (member is not null)
                    {
                        groupMembers.Add(new GroupMember { GroupId = createGroup.Id, UserId = memberId });
                    }
                    else
                    {
                        invalidMembers.Add(memberId);
                    }
                }

                // If there are invalid members, return error but still create valid ones
                if (invalidMembers.Count != 0)
                {
                    return BadRequest($"Some members not found: {string.Join(", ", invalidMembers)}");
                }

                // Add all valid members in one go
                await Context.GroupMembers.AddRangeAsync(groupMembers);
                await Context.SaveChangesAsync();

                return Ok(createGroup);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroupAsync(int groupId, Group newGroup)
        {
            var oldGroup = await Context.Groups.Where(g => g.Id == groupId).FirstOrDefaultAsync();
            if (oldGroup is null)
            {
                return NotFound();
            }
            oldGroup.Name = newGroup.Name;
            oldGroup.Description = newGroup.Description;
            return Ok(await Context.SaveChangesAsync());
        }
        [HttpPut("{groupId}/AddMembers")]
        public async Task<IActionResult> AddGroupMembers(int groupId, List<string> memberIds)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var group = await Context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                {
                    return NotFound("group not found");
                }
                var existingMemberIds = group.MemberIds;
                var newMemberIds = memberIds.Except(existingMemberIds).ToList();
                var invalidMemberIds = new List<string>();
                var newMembers = new List<GroupMember>();
                foreach (var memberId in newMemberIds)
                {
                    var user = await UserManager.FindByIdAsync(memberId);
                    if (user is null)
                    {
                        invalidMemberIds.Add(memberId);
                    }
                    else
                    {
                        newMembers.Add(new GroupMember { GroupId = groupId, UserId = memberId });
                    }
                }
                if (invalidMemberIds.Count != 0)
                {
                    return BadRequest($"Some members not found: {string.Join(", ", invalidMemberIds)}");
                }
                group.MemberIds.AddRange(newMemberIds);
                await Context.GroupMembers.AddRangeAsync(newMembers);
                await Context.SaveChangesAsync();


                return Ok(await Context.GroupMembers.AsNoTracking().Where(g => g.GroupId == groupId).ToListAsync());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }


        }
        [Authorize(Policy = "Administrator")]
        [HttpDelete("delete/{groupId}")]
        public async Task<IActionResult> DeleteGroupAsync(int groupId)
        {
            var delete = await Context.Groups.Where(g => g.Id == groupId).ExecuteDeleteAsync();
            return NoContent();
        }
        [HttpGet("chat/{groupId}")]
        public async Task<List<ChatMessage>> GetGroupConversationAsync(int groupId)
        {
            var conversation = await Context.ChatMessages.AsNoTracking().Where(c => c.ToGroupId == groupId).OrderBy(c => c.CreatedDate).ToListAsync();
            return conversation;
        }
       

    }

}
