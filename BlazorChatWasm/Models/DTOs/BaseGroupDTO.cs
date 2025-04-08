
namespace BlazorChatWasm.Models.DTOs
{
    public class BaseGroupDTO
    {
        // DTO for Group

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } 
        public IEnumerable<BaseApplicationUserDTO> MembersInfo { get; set; } = [];
        public IEnumerable<BaseApplicationUserDTO> Admins { get; set; } = [];
        public IEnumerable<BaseApplicationUserDTO> Moderators { get; set; } = [];

        public int MembersCount { get; set; }


    }
}
