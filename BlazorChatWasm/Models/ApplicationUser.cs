using BlazorChatWasm.Models.Chat;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace BlazorChatWasm.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ImageUrl { get; set; } = string.Empty;
        public List<string>? FriendIDs { get; set; }
        [JsonIgnore]
        public virtual ICollection<ApplicationUser> Friends { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesFromUsers { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesToUsers { get; set; }
        [JsonIgnore]
        public virtual ICollection<ApplicationUser> Groups { get; set; }
        public ApplicationUser()
        {
            Id = string.Empty;
            FriendIDs = [];
            Friends = new HashSet<ApplicationUser>();
            ChatMessagesFromUsers = new HashSet<ChatMessage>();
            ChatMessagesToUsers = new HashSet<ChatMessage>();
            Groups = new HashSet<ApplicationUser>();
        }
    }
}
