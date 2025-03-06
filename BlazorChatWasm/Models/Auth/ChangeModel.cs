using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.Auth
{
    public class ChangeModel
    {
        public string Id { get; set; } = "";

        [Required]
        public string OldPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords dont match")]
        public string ConfirmPassword { get; set; } = "";
    }
}
