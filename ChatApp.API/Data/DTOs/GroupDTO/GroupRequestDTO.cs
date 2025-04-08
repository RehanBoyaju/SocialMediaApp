using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ChatApp.API.Data.DTOs.GroupDTO
{
    public class GroupRequestDTO
    {
        public int groupId { get; set; }
        public string senderId { get; set; } = string.Empty;
    }
}
