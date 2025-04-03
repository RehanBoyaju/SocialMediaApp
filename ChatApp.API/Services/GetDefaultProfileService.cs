namespace ChatApp.API.Services
{

    public static class GetDefaultProfileService
    {
        private static string DefaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");

        public static string DefaultProfile()
        {
            return ImageBytesToStringService.GetProfileFromBytes(DefaultProfileBytes());
        }
        private static byte[] DefaultProfileBytes()
        {
              return File.Exists(DefaultImagePath)? File.ReadAllBytes(DefaultImagePath) : [];
        }
    }
}
