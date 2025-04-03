using BlazorChatWasm.Models.Chat;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorChatWasm.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public  string Name { get; set; }

        [Required]
        [MaxLength(1000)]
        public  string Description { get; set; }

        public string? ProfileImageUrl { get; set; }

        public List<string> MemberIds { get; set; } = [];

        public ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();

        // Computed Members Count
        public int MembersCount => Members.Count;

        [JsonIgnore]
        public ICollection<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();
    }
}
