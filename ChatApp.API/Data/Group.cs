using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.API.Data
{
    public class Group
    {
        [Required]
        public required string Id { get; set; }
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        [Required]
        [MaxLength(1000)]
        public required string Description { get; set; }
        [Required]
        [MinLength(3)]
        public IEnumerable<int> MemberIds { get; set; } = [];

        [JsonIgnore]
        public ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();
        public int MembersCount { get => Members.Count; }

        [JsonIgnore]
        public ICollection<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();


    }
}
