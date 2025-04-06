namespace ChatApp.API.Data
{
    public class FriendRequest
    {
        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public string? ReceiverId { get; set; }
        public ApplicationUser? Receiver { get; set; }
        public DateTime RequestDate { get; set; }
        public bool? IsAccepted { get; set; }
    }
}
