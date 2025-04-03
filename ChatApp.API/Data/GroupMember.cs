using System.Text.Json.Serialization;

namespace ChatApp.API.Data
{
    public class GroupMember
    {
        public int GroupId { get; set; }

        [JsonIgnore]
         public virtual Group? Group { get; set; } 
        public string UserId { get; set; } 

        [JsonIgnore]
        public virtual ApplicationUser? User { get; set; }   
    }

}
