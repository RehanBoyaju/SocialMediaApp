using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChatApp.API.Data
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public  string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public  string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public List<string> MemberIds { get; set; } = [];

        [JsonIgnore]
        public virtual ICollection<GroupMember>? Members { get; set; } = new HashSet<GroupMember>();

        public int MembersCount => Members!.Count;

        [JsonIgnore]
        public virtual ICollection<ChatMessage>? ChatMessages { get; set; } = new HashSet<ChatMessage>();

       
    }
}
