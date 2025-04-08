using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.DTOs
{
    public class GroupModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength (200)]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        [Required]
        [MinLength(3)]
        public List<string> MemberIds { get; set; } = [];

        [Required]
        [MinLength(1)]
        public virtual List<string> AdminIds { get; set; } = [];

        public virtual List<string> ModeratorIds { get; set; } = [];
    }
}
