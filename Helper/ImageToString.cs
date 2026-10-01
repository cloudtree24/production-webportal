namespace HCMPortal.Helper
{
    public static class ImageToString
    {
        /// <summary>
        /// Converts a file into a Base64 Data URI by combining the WebRootPath and the file path.
        /// </summary>
        /// <param name="webRootPath">The physical path of the wwwroot folder (from IWebHostEnvironment).</param>
        /// <param name="imageRelativePath">The path and name of the image relative to the web root (e.g., "assets/images/Cloudtree_final_Logo.png").</param>
        /// <returns>A data URI string (e.g., "data:image/png;base64,...").</returns>
        public static string GetBase64Logo(string webRootPath, string imageRelativePath)
        {
            // 1. Path.Combine is CRUCIAL: it correctly handles slashes/backslashes based on the OS.
            // We use Replace to normalize the input string (which uses '/') to the system's separator
            string imageFilePath = Path.Combine(webRootPath, imageRelativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(imageFilePath))
            {
                // File NOT found on disk.
                return string.Empty;
            }

            try
            {
                byte[] imageBytes = File.ReadAllBytes(imageFilePath);
                string base64String = Convert.ToBase64String(imageBytes);

                // 2. Infer MIME type (or make it an argument)
                string extension = Path.GetExtension(imageRelativePath).ToLower();
                string mimeType = extension == ".png" ? "image/png" : (extension == ".jpg" || extension == ".jpeg" ? "image/jpeg" : "application/octet-stream");

                // Returns the complete data URI ready for the <img src=""> tag
                return $"data:{mimeType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error converting image to Base64: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
