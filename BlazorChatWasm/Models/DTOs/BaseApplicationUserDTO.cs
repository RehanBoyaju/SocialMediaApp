﻿namespace BlazorChatWasm.Models.DTOs

{
    public class BaseApplicationUserDTO
    {

        public string Id { get; set; } = string.Empty;   // User Id
        public string UserName { get; set; } = string.Empty; // User's Name
        public string Email { get; set; } = string.Empty; // User's Email
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }
        public DateTime AddedDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;


    }
}
