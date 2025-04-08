namespace ChatApp.API.Data
{
    public class GroupRequest
    {

        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public DateTime RequestDate { get; set; }
        public bool? IsAccepted { get; set; }

    }
}
