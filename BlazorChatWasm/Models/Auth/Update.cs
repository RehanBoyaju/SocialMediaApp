using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.Auth
{
    public class UpdateModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        
    }
}
