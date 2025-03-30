using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Text.Json.Serialization;

namespace ChatApp.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Default image path constructed from WebRootPath
        //private static string DefaultImagePath => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");

        // Property for ProfileImage, initialized in the constructor
        public byte[]? ProfileImage { get; set; }

        //public IEnumerable<int> ChatMessagesIdFromUsers { get; set; } = [];
        //public IEnumerable<int> ChatMessagesIdToUsers { get; set; } = [];


        [JsonIgnore]
        public ICollection<ChatMessage> ChatMessagesFromUsers { get; set; } = new HashSet<ChatMessage>();
        [JsonIgnore]
        public  ICollection<ChatMessage> ChatMessagesToUsers { get; set; } = new HashSet<ChatMessage>();
        
        [JsonIgnore]
        public virtual ICollection<Group> Groups { get; set; } = new HashSet<Group>();
    }
}
