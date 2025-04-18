using BlazorChatWasm.Models.Chat;
using BlazorChatWasm.Models.Groups;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace BlazorChatWasm.Models.User
{
    public class ApplicationUser : IdentityUser
    {
        public string ImageUrl { get; set; } = string.Empty;
        public virtual ICollection<FriendRequest> FriendRequestsSent { get; set; } = new HashSet<FriendRequest>();
        public virtual ICollection<FriendRequest> FriendRequestsReceived { get; set; } = new HashSet<FriendRequest>();
        public virtual ICollection<GroupRequest> GroupRequestsSent { get; set; } = new HashSet<GroupRequest>();
        public virtual ICollection<ApplicationUser> Friends { get; set; }
        public virtual ICollection<ChatMessage> ChatMessagesFromUsers { get; set; }
        public virtual ICollection<ChatMessage> ChatMessagesToUsers { get; set; }
        public virtual ICollection<ApplicationUser> Groups { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }
        public DateTime DateAdded { get; set; }
        public ApplicationUser()
        {
            Id = string.Empty;
            Friends = new HashSet<ApplicationUser>();
            ChatMessagesFromUsers = new HashSet<ChatMessage>();
            ChatMessagesToUsers = new HashSet<ChatMessage>();
            Groups = new HashSet<ApplicationUser>();
        }

        public override bool Equals(object? obj)
        {
            return obj is ApplicationUser user && this.Id == user.Id;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
