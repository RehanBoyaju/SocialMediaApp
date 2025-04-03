namespace ChatApp.API.Services
{
    public static class ImageBytesToStringService
    {
        public static string GetProfileFromBytes(byte[] image)
        {
            string mimeType = GetImageMimeType(image);
            
            return $"data:{mimeType};base64,{Convert.ToBase64String(image)}";
        }
        private static string GetImageMimeType(byte[] imageBytes)
        {
            // Check for common image file signatures
            if (imageBytes.Length < 4)
                return "image/jpeg"; // Not enough data to determine so default to jpeg

            // JPEG: starts with FF D8 FF
            if (imageBytes.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
                return "image/jpeg";

            // PNG: starts with 89 50 4E 47 0D 0A 1A 0A
            if (imageBytes.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                return "image/png";

            // GIF: starts with "GIF" (47 49 46)
            if (imageBytes.Take(3).SequenceEqual(new byte[] { 0x47, 0x49, 0x46 }))
                return "image/gif";

            // WEBP: starts with "RIFF" (52 49 46 46) followed by "WEBP" (57 45 42 50)
            if (imageBytes.Length >= 12 &&
                imageBytes.Take(4).SequenceEqual(new byte[] { 0x52, 0x49, 0x46, 0x46 }) &&
                imageBytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x57, 0x45, 0x42, 0x50 }))
                return "image/webp";

            // BMP: starts with "BM" (42 4D)
            if (imageBytes.Take(2).SequenceEqual(new byte[] { 0x42, 0x4D }))
                return "image/bmp";

            // TIFF: starts with "II" (49 49) or "MM" (4D 4D)
            if (imageBytes.Take(2).SequenceEqual(new byte[] { 0x49, 0x49 }) ||
                imageBytes.Take(2).SequenceEqual(new byte[] { 0x4D, 0x4D }))
                return "image/tiff";

            return "image/jpeg";//default to jpeg
        }
    }
}
