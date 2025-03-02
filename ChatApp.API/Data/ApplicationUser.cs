using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Text.Json.Serialization;

namespace ChatApp.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        // Default image path constructed from WebRootPath
        private static string DefaultImagePath => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");

        // Property for ProfileImage, initialized in the constructor
        public byte[] ProfileImage { get; set; }

        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesFromUsers { get; set; } = new HashSet<ChatMessage>();

        [JsonIgnore]
        public virtual ICollection<ChatMessage> ChatMessagesToUsers { get; set; } = new HashSet<ChatMessage>();

        public ApplicationUser()
        {
            // Set the ProfileImage to default image bytes if the file exists, otherwise empty
            ProfileImage = File.Exists(DefaultImagePath)
                ? File.ReadAllBytes(DefaultImagePath)
                : Array.Empty<byte>();
        }
    }
}
