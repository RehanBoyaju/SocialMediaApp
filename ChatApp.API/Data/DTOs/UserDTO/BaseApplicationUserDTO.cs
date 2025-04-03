namespace ChatApp.API.Data.DTOs.UserDTO

{
    public class BaseApplicationUserDTO
    {

        public string Id { get; set; } = string.Empty;   // User Id
        public string UserName { get; set; } = string.Empty; // User's Name
        public string Email { get; set; } = string.Empty; // User's Email

        public string ProfileImageUrl { get; set; } = string.Empty; 


    }
}
