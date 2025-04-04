using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.Auth
{
    public class RegisterModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? ImageUrl { get; set; }
    }
}
