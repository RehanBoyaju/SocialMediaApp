﻿namespace ChatApp.API.Data
{
    public class Relationship
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string FriendId { get; set; }
        public ApplicationUser Friend { get; set; }
    }
}
