using BlazorChatWasm.Models.Chat;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace BlazorChatWasm.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string profileImageUrl { get; set; }
        //public byte[] profileImage { get; set; } = Array.Empty<byte>();

        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesFromUsers { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesToUsers { get; set; }
        public ApplicationUser()
        {
            ChatMessagesFromUsers = new HashSet<ChatMessage>();
            ChatMessagesToUsers = new HashSet<ChatMessage>();
            //profileImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(profileImage)}";
        }
    }
}
