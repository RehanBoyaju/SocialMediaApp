using ChatApp.API.Data.DTOs.UserDTO;

namespace ChatApp.API.Data.DTOs.GroupDTO
{
    public class BaseGroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public IEnumerable<BaseApplicationUserDTO> Moderators { get; set; } = [];
        public IEnumerable<BaseApplicationUserDTO> Admins { get; set; } = [];
        public IEnumerable<BaseApplicationUserDTO> MembersInfo { get; set; } = [];

        // The members count
        public int MembersCount { get; set; }


    }
}
