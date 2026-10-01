using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;

namespace HCMPortal.Controllers
{
    // NOTE: You might need to change the route to fit your MVC project's structure.
    // If you use standard MVC routing, it might be [Route("[controller]/[action]")]
    [Route("api/[controller]")] // Using 'api' route prefix for clarity
    [ApiController] // Ensures API-like behavior (automatic model binding, etc.)
    public class FilesController : Controller // Use Controller or ControllerBase depending on your needs
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public FilesController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// This action receives the file via HTTP POST from the AdminPanel.
        /// Expected Endpoint: POST /api/Files/UploadImage
        /// </summary>
        /// <param name="file">The IFormFile sent in the request's multipart form data.</param>
        /// <returns>A status response.</returns>
        [HttpPost("UploadImage")] // Defines the specific route suffix
                                  // You might need to add [DisableRequestSizeLimit] if files are large
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // 1. Basic Validation
            if (file == null || file.Length == 0)
            {
                // Return an HTTP 400 Bad Request to the caller (AdminPanel)
                return BadRequest(new { Message = "No file received or file is empty." });
            }

            try
            {
                // 2. Define the Target Path
                // Use WebRootPath to ensure the file is saved within the 'wwwroot' structure
                string relativeUploadPath = "assets/UploadImg"; // The path relative to wwwroot
                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, relativeUploadPath);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // 3. Get File Name and Define Final Path
                // The unique name should already be embedded in file.FileName from the AdminPanel
                string newFileName = file.FileName;
                string crmFilePath = Path.Combine(uploadFolder, newFileName);

                // 4. Save the File
                using (var stream = new FileStream(crmFilePath, FileMode.Create))
                {
                    // Asynchronously copy the incoming stream to the physical file
                    await file.CopyToAsync(stream);
                }

                // 5. Success Response
                // Return a success status (HTTP 200 OK) to the AdminPanel
                return Ok(new
                {
                    Message = "File uploaded successfully to HCMPortal.",
                    FileName = newFileName,
                    PublicUrl = $"/{relativeUploadPath}/{newFileName}" // The URL for the frontend to use
                });
            }
            catch (Exception ex)
            {
                // In case of any error (e.g., file permission issues), return an HTTP 500 error
                // Log the exception details here for debugging!
                return StatusCode(500, new { Message = "Internal server error while saving the file.", ErrorDetail = ex.Message });
            }
        }
        /// <summary>
        /// This action receives the file via HTTP POST from the AdminPanel.
        /// Expected Endpoint: POST /api/Files/UploadImage
        /// </summary>
        /// <param name="file">The IFormFile sent in the request's multipart form data.</param>
        /// <returns>A status response.</returns>
        [HttpPost("UploadDocument")] // Defines the specific route suffix
                                  // You might need to add [DisableRequestSizeLimit] if files are large
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            // 1. Basic Validation
            if (file == null || file.Length == 0)
            {
                // Return an HTTP 400 Bad Request to the caller (AdminPanel)
                return BadRequest(new { Message = "No file received or file is empty." });
            }

            try
            {
                // 2. Define the Target Path
                // Use WebRootPath to ensure the file is saved within the 'wwwroot' structure
                string relativeUploadPath = "assets/UploadDoc"; // The path relative to wwwroot
                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, relativeUploadPath);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // 3. Get File Name and Define Final Path
                // The unique name should already be embedded in file.FileName from the AdminPanel
                string newFileName = file.FileName;
                string crmFilePath = Path.Combine(uploadFolder, newFileName);

                // 4. Save the File
                using (var stream = new FileStream(crmFilePath, FileMode.Create))
                {
                    // Asynchronously copy the incoming stream to the physical file
                    await file.CopyToAsync(stream);
                }

                // 5. Success Response
                // Return a success status (HTTP 200 OK) to the AdminPanel
                return Ok(new
                {
                    Message = "File uploaded successfully to HCMPortal.",
                    FileName = newFileName,
                    PublicUrl = $"/{relativeUploadPath}/{newFileName}" // The URL for the frontend to use
                });
            }
            catch (Exception ex)
            {
                // In case of any error (e.g., file permission issues), return an HTTP 500 error
                // Log the exception details here for debugging!
                return StatusCode(500, new { Message = "Internal server error while saving the file.", ErrorDetail = ex.Message });
            }
        }



        [HttpGet]
        [Route("FetchFile")]
        public IActionResult FetchFile([FromQuery] string fileNameWithExtension)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileNameWithExtension))
                    return BadRequest("Filename is required.");


                string extension = Path.GetExtension(fileNameWithExtension).ToLowerInvariant();
                bool isImageType = extension.Equals(".jpg") || extension.Equals(".jpeg") || extension.Equals(".png") || extension.Equals(".gif");
                string relativeUploadPath = isImageType ? "assets/UploadImg" : "assets/UploadDoc";
                // NOTE: You must update this logic to correctly determine the uploadDirectory 
                // based on the file extension (Image or Document) to find the file.
                // For simplicity, we assume it's currently only checking 'UploadImg'.
                string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, relativeUploadPath);

                string safeFileName = Path.GetFileName(fileNameWithExtension);
                string filePath = Path.Combine(uploadDirectory, safeFileName);

                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found.");

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string base64String = Convert.ToBase64String(fileBytes);

                // Include the filename in the response to help AdminPanel save it.
                return Ok(new
                {
                    fileName = safeFileName, // Added for convenience
                    base64File = base64String
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = $"Error occurred: {ex.Message}" });
            }
        }
    }
}
