using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [JsonIgnore]
        public virtual ICollection<GroupMember> Members { get; set; } = new HashSet<GroupMember>();
        [JsonIgnore]
        public int MembersCount => Members!.Count;

        [NotMapped]
        public virtual List<string> MemberIds { get; set; } = []; 
        [NotMapped]
        public virtual HashSet<string> AdminIds { get; set; } = [];
        [NotMapped]
        public virtual List<string> ModeratorIds { get; set; } = [];
        [JsonIgnore]
        public virtual ICollection<GroupRequest> GroupRequestsReceived { get; set; } = new HashSet<GroupRequest>();

        [JsonIgnore]
        public virtual ICollection<ChatMessage>? ChatMessages { get; set; } = new HashSet<ChatMessage>();

        [NotMapped]
        public IEnumerable<GroupMember>? Admins  =>   Members?.Where(m => m.IsAdmin) ?? [];

        [NotMapped]
        public IEnumerable<GroupMember>? Moderators => Members?.Where(m => m.IsModerator) ?? [];

    }
}
