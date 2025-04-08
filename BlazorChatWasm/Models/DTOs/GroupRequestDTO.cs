

namespace BlazorChatWasm.Models.DTOs
{
    public class GroupRequestDTO
    {
        public int groupId { get; set; }
        public string senderId { get; set; } = string.Empty;
    }
}
