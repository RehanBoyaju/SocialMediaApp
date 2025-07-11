﻿using System.ComponentModel.DataAnnotations;

namespace BlazorChatWasm.Models.Auth
{
    public class InputModel
    {
        [Required]
        [MinLength(2)]
        public string Username { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";


        public string? ImageUrl { get; set; }

        public string[] Errors { get; set; } = [];
    }

}
