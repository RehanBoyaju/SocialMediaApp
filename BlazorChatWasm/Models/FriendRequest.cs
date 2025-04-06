namespace BlazorChatWasm.Models
{
    public class FriendRequest
    {
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = new ApplicationUser();
        public string ReceiverId { get; set; } = string.Empty;
        public ApplicationUser Receiver { get; set; } = new ApplicationUser();

        public DateTime RequestDate { get; set; }
        public bool? IsAccepted { get; set; }
    }
}
