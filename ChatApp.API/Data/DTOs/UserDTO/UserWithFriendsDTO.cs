using ChatApp.API.Data.DTOs.GroupDTO;

namespace ChatApp.API.Data.DTOs.UserDTO
{
    public class UserWithFriendsDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;

        public string ProfileImageUrl = string.Empty;

        public  ICollection<BaseApplicationUserDTO> Friends { get; set; } = new List<BaseApplicationUserDTO>();

        public  ICollection<BaseGroupDTO>? Groups { get; set; } = new List<BaseGroupDTO>();
    }
}
