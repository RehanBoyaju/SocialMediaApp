namespace ChatApp.API.Services
{

    public static class GetDefaultProfileService
    {
        private static string DefaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");
        private static string DefaultGroupImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Group.png");

        public static string DefaultProfile()
        {
            return ImageBytesToStringService.GetProfileFromBytes(DefaultProfileBytes(DefaultImagePath));
        }
        public static string DefaultGroupProfile()
        {
            return ImageBytesToStringService.GetProfileFromBytes(DefaultProfileBytes(DefaultGroupImagePath));
        }
        private static byte[] DefaultProfileBytes(string path)
        {
              return File.Exists(path)? File.ReadAllBytes(path) : [];
        }
    }
}
