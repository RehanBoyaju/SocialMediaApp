﻿using ChatApp.API.Data;

namespace ChatApp.API.Data
{
    public class ChatMessage
    {
        public long Id { get; set; }
        public string? FromUserId { get; set; }
        public string? ToUserId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public  virtual ApplicationUser? FromUser { get; set; }
        public virtual ApplicationUser? ToUser { get; set; }
        public  int? ToGroupId { get; set; }
        public virtual Group? ToGroup { get; set; }

    }
}
