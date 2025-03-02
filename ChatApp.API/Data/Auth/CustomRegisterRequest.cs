namespace ChatApp.API.Data.Auth
{
    public class CustomRegisterRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }   
        public required string Password { get; set; }
        public required IFormFile? ProfileImage { get; set; }

}
}
