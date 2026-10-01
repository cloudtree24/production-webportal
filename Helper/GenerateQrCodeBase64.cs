using QRCoder;
using System.Drawing; // Required for Bitmap
using System.Drawing.Imaging;
using System.IO;
namespace HCMPortal.Helper
{
    public static class GenerateQrCodeBase64
    {
        public static string GenerateQrCode(string url)
        {
            // Define the MIME type for PNG
            const string MimeType = "image/png";

            using (var generator = new QRCodeGenerator())
            {
                // 1. Generate the QR Code Data
                var qrCodeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                // 2. Create the QR Code Bitmap (Image)
                using (var qrCode = new QRCode(qrCodeData))
                {
                    // GetGraphic(pixelsPerModule)
                    using (var qrCodeImage = qrCode.GetGraphic(10))
                    {
                        // 3. Convert Image to Byte Array using a MemoryStream
                        using (var ms = new MemoryStream())
                        {
                            // Save the image data into the memory stream as PNG format
                            qrCodeImage.Save(ms, ImageFormat.Png);

                            // Get the byte array from the stream
                            byte[] imageBytes = ms.ToArray();

                            // 4. Convert the byte array to a Base64 string
                            string base64String = Convert.ToBase64String(imageBytes);

                            // 5. Return the complete Data URI string
                            return $"data:{MimeType};base64,{base64String}";
                        }
                    }
                }
            }
            // Note: The IWebHostEnvironment 'env' is no longer needed in this version
        }
    }
}
