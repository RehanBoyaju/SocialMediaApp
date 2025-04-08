using BlazorChatWasm.Models.Chat;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BlazorChatWasm.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public  string? Name { get; set; }

        [Required]
        [MaxLength(1000)]
        public  string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<string> MemberIds { get; set; } = [];

        public virtual List<string> AdminIds { get; set; } = [];

        public virtual List<string> ModeratorIds { get; set; } = [];

        public ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();

        public int MembersCount => Members.Count;
        public virtual ICollection<GroupRequest> GroupRequestsReceived { get; set; } = new HashSet<GroupRequest>();

        [JsonIgnore]
        public ICollection<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();

        public ICollection<ApplicationUser> Admins { get; set; }  = new HashSet<ApplicationUser>();

        public ICollection<ApplicationUser> Moderators { get; set; } = new HashSet<ApplicationUser>();
    }
}
