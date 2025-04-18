using ChatApp.API.Data;
using ChatApp.API.Data.DTOs.GroupDTO;
using ChatApp.API.Data.DTOs.UserDTO;
using ChatApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : ControllerBase
    {
        public UserManager<ApplicationUser> UserManager { get; } = userManager;
        public ApplicationDbContext Context { get; } = context;

        [HttpPut("join")]
        public async Task<FormResult> JoinGroupAsync([FromBody]int groupId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                var group = await Context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
                }
                if (group.Members!.Any(m => m.UserId == userId))
                {
                    return new FormResult() { Succeeded = false, Errors = ["Already a member"] };
                }
                //group.Members!.Add(new GroupMember { GroupId = groupId, UserId = userId });
                group.GroupRequestsReceived.Add(new GroupRequest() { GroupId = groupId, SenderId = userId, RequestDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
            }
            finally
            {
                await Context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            return new FormResult() { Succeeded = true, Errors = null };
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroupsAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
            var groups = await Context.Groups.AsNoTracking().Include(g=> g.Members).ToListAsync();
            return Ok(groups);
        }
        [HttpGet("other")]
        public async Task<IActionResult> GetOtherGroupsAsync()
        {
            var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
            var user = await Context.Users.AsNoTracking().Where(u => u.Id == userId).FirstOrDefaultAsync();

            //var groups = await Context.Groups
            //    .AsNoTracking()
            //    .Where(g => !g.MemberIds.Contains(userId))
            //    .ToListAsync();
            //var groups = await Context.GroupMembers.AsNoTracking()
            //    .Include(g => g.Group)
            //    .Where(g => g.UserId != userId && !Context.GroupMembers.Any(m => m.UserId == userId && m.GroupId  == g.GroupId)).ToListAsync();

            //return Ok(groups);
            //var allGroups = ((List<ApplicationUser>)await GetAllGroupsAsync()).ToHashSet();
            //var userGroups = ((List<ApplicationUser>)await GetGroupsAsync(userId)).ToHashSet();
            //var otherGroups  = allGroups.Except(userGroups).ToList();

            var userGroupIds = await Context.GroupMembers
                                                                    .Where(gm => gm.UserId == userId)
                                                                    .Select(gm => gm.GroupId)
                                                                    .ToListAsync();

            var otherGroups = await Context.Groups
                .Where(g => !userGroupIds.Contains(g.Id))
                .ToListAsync();

            return Ok(otherGroups);
        }

        [HttpGet("all/{userId}")]
        public async Task<IActionResult> GetGroupsAsync(string userId)
        {
            //var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
            var user = await Context.Users.AsNoTracking().Where(u => u.Id == userId).FirstOrDefaultAsync();
            var groups = await Context.GroupMembers.AsNoTracking().Include(g => g.Group).Where(g => g.UserId == userId).Select(g => g.Group).ToListAsync();

            return Ok(groups);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupByIdAsync(int groupId)
        {

            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                bool isMember = await Context.GroupMembers
                                                                 .AsNoTracking()
                                                                 .AnyAsync(g => g.UserId == userId && g.GroupId == groupId);

                if (!isMember)
                {
                    //return new FormResult { Succeeded = false,Errors=  ["Not found"]};
                    return Unauthorized();

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
                    Admins = group.Admins.Select(m => new BaseApplicationUserDTO
                    {
                        Id = m.User.Id,
                        UserName = m.User.UserName,
                        Email = m.User.Email,
                        ImageUrl = m.User.ImageUrl,
                        IsAdmin = true,
                        IsModerator = false,
                        AddedDate = m.AddedDate
                    }),
                    Moderators = group.Moderators.Select(m => new BaseApplicationUserDTO
                    {
                        Id = m.User.Id,
                        UserName = m.User.UserName,
                        Email = m.User.Email,
                        ImageUrl = m.User.ImageUrl,
                        IsAdmin = false,
                        IsModerator = true,
                        AddedDate = m.AddedDate
                    }),
                    MembersInfo = group.Members.Select(m => new BaseApplicationUserDTO
                    {
                        Id = m.User.Id,
                        UserName = m.User.UserName,
                        Email = m.User.Email,
                        ImageUrl = m.User.ImageUrl,
                        IsAdmin = m.IsAdmin,
                        IsModerator = m.IsModerator,
                        AddedDate = m.AddedDate
                    }).ToList(),
                    MembersCount = group.Members.Count
                };
            return Ok(groupDto);

            }
            catch(Exception ex)
            {
                throw new Exception (ex.Message);
            }
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
                var groupAdmins = new List<GroupMember>();
                
                foreach (var memberId in createGroup.MemberIds)
                {
                    var member = await UserManager.FindByIdAsync(memberId);
                    if (member is not null)
                    {
                        if (!createGroup.AdminIds.Contains(memberId))
                        {
                            groupMembers.Add(new GroupMember { GroupId = createGroup.Id, UserId = memberId, AddedDate = DateTime.Now });
                        }
                        else
                        {
                            groupMembers.Add(new GroupMember { GroupId = createGroup.Id, UserId = memberId, AddedDate = DateTime.Now ,IsAdmin = true});

                        }

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
        public async Task<FormResult> UpdateGroupAsync(int groupId, [FromBody] Group newGroup)
        {
            var oldGroup = await Context.Groups.Where(g => g.Id == groupId).FirstOrDefaultAsync();
            if (oldGroup is null)
            {
                return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
            }
            oldGroup.Name = newGroup.Name;
            oldGroup.Description = newGroup.Description;
            if (!string.IsNullOrEmpty(newGroup.ImageUrl))
            {
                oldGroup.ImageUrl = newGroup.ImageUrl;

            }
            await Context.SaveChangesAsync();
            if (newGroup.AdminIds.Count > 0)
            {
                var addAdmins = await AddGroupAdmins(groupId, newGroup.AdminIds);
                if (addAdmins.Succeeded == false)
                {
                    return addAdmins;
                }
            }
            if (newGroup.ModeratorIds.Count > 0)
            {
                var addModerators = await AddGroupModerators(groupId, newGroup.ModeratorIds);
                if (addModerators.Succeeded == false)
                {
                    return addModerators;
                }
            }
            if (newGroup.MemberIds.Count != 0)
            {
                var addMembers = await AddGroupMembers(groupId, newGroup.MemberIds);
                if (addMembers.Succeeded == false)
                {
                    return addMembers;
                }
            }
            
            return new FormResult() { Succeeded = true, Errors = null };

        }

        //[HttpPut("{groupId}/AddMembers")]
        public async Task<FormResult> AddGroupMembers(int groupId, List<string> memberIds)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var group = await Context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
                }
                var existingMemberIds = await Context.GroupMembers.Where(u=> u.GroupId ==group.Id).Select( g=> g.UserId).ToHashSetAsync();
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

                    return new FormResult() { Succeeded = false, Errors = ["Some members not found: {string.Join(", ", invalidMemberIds)}"] };
                }
                //group.MemberIds.AddRange(newMemberIds);
                await Context.GroupMembers.AddRangeAsync(newMembers);
                
                await Context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new FormResult() { Succeeded = true, Errors = null };
                //return Ok(await Context.GroupMembers.AsNoTracking().Where(g => g.GroupId == groupId).ToListAsync());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
            }


        }

        //[HttpPut("{groupId}/add/admin")]
        public async Task<FormResult> AddGroupAdmins(int groupId, List<string> adminIds)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var group = await Context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
                }
                var existingAdminIds = await Context.GroupMembers.Where(u => u.GroupId == group.Id && u.IsAdmin).Select(g => g.UserId).ToHashSetAsync();
                var newAdminIds = adminIds.Except(existingAdminIds).ToList();
                var invalidAdminIds = new List<string>();
                var newAdmins = new List<GroupMember>();
                foreach (var memberId in newAdminIds)
                {
                    var user = await UserManager.FindByIdAsync(memberId);
                    if (user is null)
                    {
                        invalidAdminIds.Add(memberId);
                    }
                    else
                    {
                        var member = await Context.GroupMembers.FirstOrDefaultAsync(g => g.GroupId == groupId && g.UserId == memberId);
                        if (member is not null)
                        {
                            member.IsAdmin = true;
                        }
                        else
                        {
                            newAdmins.Add(new GroupMember { GroupId = groupId, UserId = memberId, IsAdmin = true });
                        }
                    }
                }
                if (invalidAdminIds.Count != 0)
                {

                    return new FormResult() { Succeeded = false, Errors = [$"Some members not found: {string.Join(", ", invalidAdminIds)}"] };
                }
                //group.MemberIds.AddRange(newMemberIds);
                if (newAdmins.Count > 0)
                {
                    await Context.GroupMembers.AddRangeAsync(newAdmins);
                }
                await Context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new FormResult() { Succeeded = true, Errors = null };
                //return Ok(await Context.GroupMembers.AsNoTracking().Where(g => g.GroupId == groupId).ToListAsync());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
            }


        }

        public async Task<FormResult> AddGroupModerators(int groupId, List<string> moderatorIds)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var group = await Context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
                if (group is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
                }
                var existingModeratorIds = await Context.GroupMembers.Where(u => u.GroupId == group.Id && u.IsModerator).Select(g => g.UserId).ToHashSetAsync();
                var newModeratorIds = moderatorIds.Except(existingModeratorIds).ToList();
                var invalidModeratorIds = new List<string>();
                var newModerators = new List<GroupMember>();
                foreach (var memberId in newModeratorIds)
                {
                    var user = await UserManager.FindByIdAsync(memberId);
                    if (user is null)
                    {
                        invalidModeratorIds.Add(memberId);
                    }
                    else
                    {
                        var member = await Context.GroupMembers.FirstOrDefaultAsync(g => g.GroupId == groupId && g.UserId == memberId);
                        if (member is not null)
                        {
                            member.IsModerator = true;
                        }
                        else
                        {
                            newModerators.Add(new GroupMember { GroupId = groupId, UserId = memberId, IsModerator = true });

                        }
                    }
                }
                if (invalidModeratorIds.Count != 0)
                {

                    return new FormResult() { Succeeded = false, Errors = [$"Some members not found: {string.Join(", ", invalidModeratorIds)}"] };
                }
                if (newModerators.Count > 0)
                {
                    await Context.GroupMembers.AddRangeAsync(newModerators);
                }
                await Context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new FormResult() { Succeeded = true, Errors = null };
                //return Ok(await Context.GroupMembers.AsNoTracking().Where(g => g.GroupId == groupId).ToListAsync());
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
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

        [HttpDelete("leave/{groupId}")]
        public async Task<FormResult> LeaveGroupAsync(int groupId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)!.Value;
                if (userId is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["User not found"] };
                }
                var group = await Context.GroupMembers.FirstOrDefaultAsync(g => g.GroupId == groupId && g.UserId == userId);

                if (group is null)
                {
                    return new FormResult() { Succeeded = false, Errors = ["Group not found"] };
                }
                Context.GroupMembers.Remove(group);
                await Context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new FormResult() { Succeeded = false, Errors = [ex.Message] };
            }
            finally
            {
                await transaction.CommitAsync();
            }
            return new FormResult() { Succeeded = true, Errors = null };

        }

    }

}
