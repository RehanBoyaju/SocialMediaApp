namespace ChatApp.API.Data
{
    public class Friendship 
    {
        public string? UserId { get; set; }


        public ApplicationUser User { get; set; }
        public string? FriendId { get; set; }

        public ApplicationUser? Friend { get; set; }
    }
}
