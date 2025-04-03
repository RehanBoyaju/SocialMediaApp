namespace ChatApp.API.Services
{
    public static class ImageToByteArrayService
    {
        public static byte[] ConvertToByteArray(string? base64String)
        {
            string DefaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");

            if (string.IsNullOrEmpty(base64String))
            {
                return System.IO.File.Exists(DefaultImagePath) ? System.IO.File.ReadAllBytes(DefaultImagePath) : [];
            }
            // Remove the data prefix
            if (base64String.Contains(','))
            {
                base64String = base64String.Split(',')[1];
            }

            return Convert.FromBase64String(base64String);
        }
    }
}
