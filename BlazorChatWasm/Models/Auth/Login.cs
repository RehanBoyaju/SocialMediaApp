using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.Auth
{
    public class LoginModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string? TwoFactorCode { get; set; }

        public string? TwoFactorRecoveryCode { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }
    }
}
