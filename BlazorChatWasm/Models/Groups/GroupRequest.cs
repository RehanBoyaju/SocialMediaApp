using BlazorChatWasm.Models.User;

namespace BlazorChatWasm.Models.Groups
{
    public class GroupRequest
    {

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = new ApplicationUser();
        public int GroupId { get; set; }
        public Group Group { get; set; } = new Group();
        public DateTime RequestDate { get; set; }
        public bool? IsAccepted { get; set; }

    }
}
