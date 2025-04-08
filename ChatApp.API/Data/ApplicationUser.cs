using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;

namespace ChatApp.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string ImageUrl { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<FriendRequest> FriendRequestsSent { get; set; } = new HashSet<FriendRequest>();
        [JsonIgnore]
        public virtual ICollection<FriendRequest> FriendRequestsReceived{ get; set; } = new HashSet<FriendRequest>();
    
        [JsonIgnore]
        public virtual ICollection<GroupRequest> GroupRequestsSent { get; set; } = new HashSet<GroupRequest>();

        [JsonIgnore]
        public virtual ICollection<Relationship> Friends { get; set; } = new HashSet<Relationship>();
        [JsonIgnore]
        public virtual ICollection<ChatMessage>? ChatMessagesFromUsers { get; set; } = new HashSet<ChatMessage>();
        [JsonIgnore]
        public  virtual ICollection<ChatMessage>? ChatMessagesToUsers { get; set; } = new HashSet<ChatMessage>();
        [JsonIgnore]
        public virtual ICollection<GroupMember>? Groups { get; set; } = new HashSet<GroupMember>();
    }
}
