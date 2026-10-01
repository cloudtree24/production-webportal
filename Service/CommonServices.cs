using HCMPortal.Models;
using HCMPortal.Helper;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Mail;
using System.Text;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using Microsoft.Extensions.Hosting.Internal;
using System.IO;


namespace HCMPortal.Service
{
    public class CommonServices
    {
        public async Task<SeoConfigViewModel> GetActiveSEOContent()
        {
            SeoConfigViewModel seoConfig = new SeoConfigViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SEOContentManagement/GetActiveSEOContent"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        seoConfig = JsonConvert.DeserializeObject<SeoConfigViewModel>(apiResponse) ?? new SeoConfigViewModel();
                    }
                    else
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return seoConfig;
        }
        public async Task<BlogViewModel> GetPagedBlogs(int page, int pageSize)
        {
            BlogViewModel blogviewmodel = new BlogViewModel();
            int totalCount = 0;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "BlogManagement/GetPagedBlogs/"+page+"/"+pageSize))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        blogviewmodel = JsonConvert.DeserializeObject<BlogViewModel>(apiResponse) ?? new BlogViewModel();
                    }
                    else
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return blogviewmodel;
        }


        public async Task<List<BlogViewModel>> GetAllActiveBlogList()
        {
            List<BlogViewModel> blogList = new List<BlogViewModel>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "BlogManagement/GetActiveBlogList"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        blogList = JsonConvert.DeserializeObject<List<BlogViewModel>>(apiResponse) ?? new List<BlogViewModel>();
                    }
                    else
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return blogList;
        }
        //getting all cancel request details
        public async Task<List<CancelSubscriptionDetailsViewModel>> CancelationRequestList()
        {
            List<CancelSubscriptionDetailsViewModel> cancelsubscriptionall = new();
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetAllCancelSubscription");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    cancelsubscriptionall = JsonConvert.DeserializeObject<List<CancelSubscriptionDetailsViewModel>>(apiResponse) ?? new List<CancelSubscriptionDetailsViewModel>();
                }
            }
            return cancelsubscriptionall;
        }
        public string GenerateOrderNumber()
        {
            // Prefix
            string prefix = "TCTO";

            // Current Month (short), Day, and Year
            string monthDayYear = DateTime.Now.ToString("MMMyydd").ToUpper();
            // Example: "SEP122025"

            // Random 6-digit number
            Random random = new Random();
            int randomNumber = random.Next(100000, 999999); // always 6 digits

            // Combine all
            string orderNo = $"{prefix}-{monthDayYear}{randomNumber}";
            return orderNo;
        }
        public List<Country> CountryList()
        {
            List<Country> obgCountry = new List<Country>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CountryState/GetAllCountry").Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            obgCountry = JsonConvert.DeserializeObject<List<Country>>(apiResponse) ?? new List<Country>();
                        }
                    }
                }
                return obgCountry;
            }
            catch
            {
                return obgCountry;
            }
        }
        public List<State> StateList(int CountryId)
        {
            List<State> obgStates = new List<State>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CountryState/GetStatesByCountry/" + CountryId).Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            obgStates = JsonConvert.DeserializeObject<List<State>>(apiResponse) ?? new List<State>();
                        }
                    }
                }
                return obgStates;
            }
            catch
            {
                return obgStates;
            }
        }
        public List<State> StateList()
        {
            List<State> obgStates = new List<State>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CountryState/GetAllState/").Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            obgStates = JsonConvert.DeserializeObject<List<State>>(apiResponse) ?? new List<State>();
                        }
                    }
                }
                return obgStates;
            }
            catch
            {
                return obgStates;
            }
        }
        public List<IndustryType> IndustyList()
        {
            List<IndustryType> industryType = new List<IndustryType>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/GetIndustryMasters").Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            industryType = JsonConvert.DeserializeObject<List<IndustryType>>(apiResponse) ?? new List<IndustryType>();
                        }
                    }
                }
                return industryType;
            }
            catch
            {
                return industryType;
            }
        }
        public ResponseModel IsMobileNoExist(string Phone, string OldPhone)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (!Phone.Equals(OldPhone))
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsPMobileNoExist/" + Phone).Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = response.Content.ReadAsStringAsync().Result;
                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse);
                            }
                        }
                    }
                    if (responseModel == null)
                        responseModel = new ResponseModel();
                }
                return responseModel;

            }
            catch (Exception)
            {
                return new ResponseModel();
            }
        }
        public ResponseModel IsEmailExist(string Email, string OldEmail)
       {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (!Email.Equals(OldEmail))
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/IsPEmailExist/" + Email).Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = response.Content.ReadAsStringAsync().Result;
                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse);
                            }
                        }
                        if (responseModel==null)
                            responseModel = new ResponseModel();
                    }
                }
                return responseModel;
            }
            catch (Exception)
            {
                return new ResponseModel();
            }
        }
        public ResponseModel IsMobileExist(string Phone, string OldPhone)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                if (!Phone.Equals(OldPhone))
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/IsPMobileNoExist/" + Phone).Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = response.Content.ReadAsStringAsync().Result;
                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse);
                            }
                        }
                        if (responseModel==null)
                            responseModel = new ResponseModel();
                    }
                }
                return responseModel;
            }
            catch (Exception)
            {
                return new ResponseModel();
            }
        }
        public string UploadFile(IFormFile file, string existingFilePath, IWebHostEnvironment hostingenvironment)
        {
            try
            {
                string filepath = string.Empty;
                string myUniqueFileName = string.Empty;
                string newFileName = string.Empty;
                if (file != null)
                {
                    if (!String.IsNullOrEmpty(existingFilePath))
                    {
                        string filePath = Path.Combine(hostingenvironment.WebRootPath, FileLocation.ImageUploadFolder, existingFilePath);
                        System.IO.File.Delete(filePath);
                    }

                    if (file.Length > 0)
                    {
                        //Getting FileName
                        var fileName = Path.GetFileName(file.FileName);

                        //Assigning Unique Filename (Guid)
                        myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss"); //Convert.ToString(Guid.NewGuid());

                        //Getting file Extension
                        var fileExtension = Path.GetExtension(fileName);

                        // concatenating  FileName + FileExtension
                        newFileName = String.Concat(myUniqueFileName, fileExtension);

                        // string UploadFolder = Path.Combine(hostingenvironment.WebRootPath, FileLocation.ImageUploadFolder);
                        string uploadFolder = Path.Combine(hostingenvironment.WebRootPath,
     fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
     fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
     fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
     fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
         ? FileLocation.ImageUploadFolder
         : FileLocation.DocumentUploadFolder);
                        filepath = Path.Combine(uploadFolder, newFileName);

                        using (FileStream fs = System.IO.File.Create(filepath))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                    }
                }
                else
                {
                    newFileName = existingFilePath;
                }
                return newFileName;

            }
            catch (Exception)
            {
                throw;
            }
        }
        //New one
        //public async Task<string> UploadFile(IFormFile file, string existingFilePath,
        //                                  IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS = false)
        //{
        //    string newFileName = string.Empty;

        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            return existingFilePath; // nothing uploaded
        //        }

        //        // Delete old file if exists
        //        if (!string.IsNullOrEmpty(existingFilePath))
        //        {
        //            string oldPath = Path.Combine(hostingEnvironment.WebRootPath,
        //                                          FileLocation.ImageUploadFolder,
        //                                          existingFilePath);

        //            if (System.IO.File.Exists(oldPath))
        //            {
        //                System.IO.File.Delete(oldPath);
        //            }
        //        }

        //        // Generate unique filename
        //        string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
        //        string extension = Path.GetExtension(file.FileName);
        //        newFileName = $"{myUniqueFileName}{extension}";

        //        // Decide folder
        //        string uploadFolder = Path.Combine(
        //            hostingEnvironment.WebRootPath,
        //            (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //             extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //             extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //             extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        //                ? FileLocation.ImageUploadFolder
        //                : FileLocation.DocumentUploadFolder);

        //        Directory.CreateDirectory(uploadFolder);

        //        // Final destination in wwwroot
        //        string finalPath = Path.Combine(uploadFolder, newFileName);

        //        // 🔥 1️⃣ Save to a TEMP file OUTSIDE wwwroot (no conflict)
        //        string tempFile = Path.GetTempFileName();

        //        using (var tempStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
        //        {
        //            await file.CopyToAsync(tempStream);
        //        }

        //        // 🔥 2️⃣ Move temp file into wwwroot (instant, no locking)
        //        System.IO.File.Move(tempFile, finalPath, true);

        //        return newFileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error inside your wwwroot
        //        var logPath = Path.Combine(hostingEnvironment.WebRootPath, "upload_log.txt");
        //        System.IO.File.AppendAllText(logPath,
        //            $"[{DateTime.Now}] ERROR: {ex.Message} {ex.StackTrace}\n");

        //        return newFileName;
        //    }
        //}

        //old one
        public async Task<string> UploadFile(IFormFile file, string existingFilePath, IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS = false)
        {
            string newFileName = string.Empty;

            try
            {
                if (file == null || file.Length == 0)
                {
                    // Keep existing file if nothing new is uploaded
                    return existingFilePath;
                }

                // Delete old file from current project if present
                if (!string.IsNullOrEmpty(existingFilePath))
                {
                    // Assuming FileLocation.ImageUploadFolder is correct for existingFilePath
                    string oldFilePath = Path.Combine(hostingEnvironment.WebRootPath, FileLocation.ImageUploadFolder, existingFilePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Generate unique file name
                string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileExtension = Path.GetExtension(file.FileName);
                newFileName = $"{myUniqueFileName}{fileExtension}";

                // Decide upload folder (image/doc)
                string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
                    fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                        ? FileLocation.ImageUploadFolder
                        : FileLocation.DocumentUploadFolder);

                Directory.CreateDirectory(uploadFolder);

                // Use MemoryStream so content can be reused
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // 1️⃣ Copy file to memory once
                    await file.CopyToAsync(memoryStream);

                    // 2️⃣ Save in current project
                    memoryStream.Position = 0;
                    string filePath = Path.Combine(uploadFolder, newFileName);
                    //
                    //using (FileStream fs = new FileStream(
                    //filePath,
                    //FileMode.Create,
                    //FileAccess.Write,
                    //FileShare.ReadWrite))   // ← THIS FIXES THE LOCKING
                    //{
                    //    await memoryStream.CopyToAsync(fs);
                    //}
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await memoryStream.CopyToAsync(fs);
                    }


                }

                return newFileName;
            }
            //To check the file not store server--Jayanta(29-11-2025)
            catch (Exception ex)
            {
                var logPath = Path.Combine(hostingEnvironment.WebRootPath, "upload_log.txt");
                System.IO.File.AppendAllText(logPath,
                    $"[{DateTime.Now}] ERROR: {ex.Message} {ex.StackTrace}\n");

                return newFileName;
            }
            //catch (Exception ex)
            //{
            //    // Log general exceptions (e.g., file system errors on the client side)
            //    // **TODO: Replace System.Diagnostics.Debug.WriteLine with your actual logging framework**
            //    System.Diagnostics.Debug.WriteLine($"General Exception in UploadFile: {ex.Message}");
            //    return newFileName;
            //}
        }

        // The parameter 'isToUploadInAMS' should be renamed to 'isToUploadInAdminPanel' or similar
        // for clarity, but we will keep it as 'isToUploadInAMS' to match your signature.
        // Keeping the original signature: 'isToUploadInAMS' means 'isToUploadInAdminPanel'
        public async Task<string> UploadFile(IFormFile file, IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS)
        {
            string newFileName = string.Empty;

            try
            {
                if (file == null || file.Length == 0)
                    return string.Empty;

                // 1. Generate unique file name and determine file type
                string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileExtension = Path.GetExtension(file.FileName);
                newFileName = $"{myUniqueFileName}{fileExtension}";

                bool isImageType = fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                   fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                   fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                   fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);


                // 2. Define Local HCMPortal Upload Folder
                string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
                    isImageType ? FileLocation.ImageUploadFolder : FileLocation.DocumentUploadFolder);

                Directory.CreateDirectory(uploadFolder);

                // 3. Use MemoryStream for content handling (read multiple times)
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Copy IFormFile content into MemoryStream (FIRST READ)
                    await file.CopyToAsync(memoryStream);

                    // Rewind stream for Local Save
                    memoryStream.Position = 0;

                    // 4. Save file in HCMPortal (Local Save)
                    string localFilePath = Path.Combine(uploadFolder, newFileName);
                    using (FileStream fs = new FileStream(localFilePath, FileMode.Create))
                    {
                        memoryStream.CopyTo(fs);
                    }

                    // 5. API upload logic to AdminPanel
                    //if (isToUploadInAMS)
                    //{
                    //    // Construct the AdminPanel API URL based on file type
                    //    string adminPanelApiBaseUrl = Common.ImageAndDocSavePageAddress();
                    //    string apiAction = isImageType ? "ReceiveImageFromPortal" : "ReceiveDocumentFromPortal";

                    //    // Full URL: e.g., http://localhost:7285/api/FileReceiver/ReceiveImageFromPortal
                    //    string adminPanelApiUrl = $"{adminPanelApiBaseUrl}FileReceiver/{apiAction}";

                    //    // Rewind the stream for the API POST (REQUIRED)
                    //    memoryStream.Position = 0;

                    //    using (var client = new HttpClient())
                    //    using (var content = new MultipartFormDataContent())
                    //    {
                    //        client.Timeout = TimeSpan.FromHours(2);
                    //        var streamContent = new StreamContent(memoryStream);
                    //        // Use the incoming file's ContentType
                    //        streamContent.Headers.Add("Content-Type", file.ContentType);

                    //        // Add the stream content using the unique filename
                    //        content.Add(streamContent, name: "file", fileName: newFileName);

                    //        // Send the request
                    //        HttpResponseMessage response = await client.PostAsync(adminPanelApiUrl, content);

                    //        // Check the response
                    //        if (!response.IsSuccessStatusCode)
                    //        {
                    //            // Log the API failure!
                    //            // _logger.LogError($"Failed to upload file to AdminPanel API. Status: {response.StatusCode}");
                    //            return String.Empty; // Return empty string on failure
                    //        }
                    //    }
                    //}
                }

                // 6. Return the unique filename on success
                return newFileName;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here! 
                // _logger.LogError(ex, "Error during file upload process in HCMPortal.");
                return newFileName;
            }
        }




        //    public async Task<string> UploadFile(IFormFile file, IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS)
        //    {
        //        // The AdminPanel project's local file name (generated earlier)
        //        string newFileName = string.Empty;

        //        try
        //        {
        //            if (file == null || file.Length == 0)
        //                return string.Empty;

        //            // Generate unique file name
        //            string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
        //            string fileExtension = Path.GetExtension(file.FileName);
        //            newFileName = $"{myUniqueFileName}{fileExtension}";

        //            // AdminPanel upload folder
        //            //string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, FileLocation.ImageUploadFolder);
        //            string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
        //fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
        //    ? FileLocation.ImageUploadFolder
        //    : FileLocation.DocumentUploadFolder);

        //            Directory.CreateDirectory(uploadFolder);

        //            // Use a MemoryStream to store the content so it can be read multiple times
        //            using (MemoryStream memoryStream = new MemoryStream())
        //            {
        //                // 🛑 FIX 1: COPY the IFormFile content into the MemoryStream (FIRST READ)
        //                await file.CopyToAsync(memoryStream);

        //                // 🛑 FIX 2: Rewind the stream for the first write (Local AdminPanel Save)
        //                memoryStream.Position = 0;

        //                // Save file in AdminPanel
        //                string adminFilePath = Path.Combine(uploadFolder, newFileName);
        //                using (FileStream fs = new FileStream(adminFilePath, FileMode.Create))
        //                {
        //                    // The memoryStream is copied to the local disk
        //                    memoryStream.CopyTo(fs);
        //                }

        //                if (isToUploadInAMS)
        //                {
        //                    bool isImageType = fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
        //                    // CRM upload folder (direct path)
        //                    string uploadFolderInCRM = "D:\\HCM\\HCM-v3.9.1\\HCM_TESTING_SERVER\\AMS_Frontend\\AdminPanel\\wwwroot\\assets\\" + ((isImageType) ? "UploadImg" : "UploadDoc");
        //                    Directory.CreateDirectory(uploadFolderInCRM);
        //                    // Save file in HCm Portal
        //                    string crmFilePath = Path.Combine(uploadFolderInCRM, newFileName);
        //                    using (FileStream fs = new FileStream(crmFilePath, FileMode.Create))
        //                    {
        //                        file.CopyTo(fs);
        //                    }
        //                }
        //                //                if (isToUploadInHCMPortal)
        //                //                {
        //                //                    bool isImageType = fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
        //                //                    string hcmPortalApiUrl = Common.ImageAndDocSavePageAddress() + ((isImageType) ? "FileReceiver/ReceiveImageFile" : "FileReceiver/ReceiveDocFile");
        //                //                    await SendFileToProjectB(file, hcmPortalApiUrl);

        //                // API upload logic
        //                //                if (isToUploadInHCMPortal)
        //                //                {
        //                //                    bool isImageType = fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //                //fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
        //                //                    string hcmPortalApiUrl = Common.ImageAndDocSavePageAddress() + ((isImageType) ? "FilesUpload/UploadImage" : "FilesUpload/UploadDocument");

        //                //                    memoryStream.Position = 0;

        //                //                    using (var client = new HttpClient())
        //                //                    using (var content = new MultipartFormDataContent())
        //                //                    {
        //                //                        var streamContent = new StreamContent(memoryStream);
        //                //                        streamContent.Headers.Add("Content-Type", file.ContentType);

        //                //                        content.Add(streamContent, name: "file", fileName: newFileName);

        //                //                        HttpResponseMessage response = await client.PostAsync(hcmPortalApiUrl, content);

        //                //                        if (!response.IsSuccessStatusCode)
        //                //                        {
        //                //                            return String.Empty;
        //                //                        }
        //                //                    }
        //                //                }
        //            } // memoryStream is disposed here

        //            return newFileName;
        //        }
        //        catch (Exception)
        //        {
        //            // Log the exception (ex) here! 
        //            return string.Empty;
        //        }
        //    }
        public async Task<bool> SendFileToProjectB(IFormFile file, string targetApiUrl)
        {
            // 1. Basic Validation
            if (file == null || file.Length == 0)
            {
                // Log or handle the case where no file is provided
                return false;
            }

            // Get the stream from the IFormFile. 
            // Since CopyToAsync was likely called before, we'll open a new stream or reset the position.
            // The most robust way is to copy it to a MemoryStream first to ensure reusability, 
            // but here we assume the IFormFile stream is ready to be read from the start.
            // If the IFormFile came directly from an API receiving it, GetStream() is the way to access it.

            try
            {
                // Use a MemoryStream to hold the file content, ensuring the stream can be reset if needed.
                // This is safer than using file.OpenReadStream() directly if the stream's position is unknown.
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset the stream to the beginning

                    using (var client = new HttpClient())
                    using (var content = new MultipartFormDataContent())
                    {
                        // Create StreamContent from the MemoryStream
                        using (var streamContent = new StreamContent(memoryStream))
                        {
                            // Add headers
                            streamContent.Headers.Add("Content-Type", file.ContentType);

                            // Add the file part. The name "file" must match the receiving API parameter (IFormFile file)
                            content.Add(streamContent, name: "file", fileName: file.FileName);

                            // Optional: Set a timeout if connection issues persist
                            // client.Timeout = TimeSpan.FromSeconds(300);

                            // Send the POST request
                            HttpResponseMessage response = await client.PostAsync(targetApiUrl, content);

                            if (response.IsSuccessStatusCode)
                            {
                                return true;
                            }
                            else
                            {
                                // Log API failure details
                                string failureDetails = await response.Content.ReadAsStringAsync();
                                // System.Diagnostics.Debug.WriteLine($"API failed: {response.StatusCode}. Details: {failureDetails}");
                                return false;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log connection failure (e.g., 'The response ended prematurely.')
                // System.Diagnostics.Debug.WriteLine($"Connection error: {ex.Message}. Inner: {ex.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Log general exceptions
                // System.Diagnostics.Debug.WriteLine($"General error: {ex.Message}");
                return false;
            }
        }

        // Example usage in Project A:
        // string apiAddress = "http://projectb.com/api/FileReceiver/ReceiveFile"; // Replace with actual address
        // string localPath = Path.Combine(hostingEnvironment.WebRootPath, "path/to/my/image.jpg");
        // bool success = await SendFileToProjectB(localPath, apiAddress);


        //public async Task<string> UploadFile(IFormFile file, string existingFilePath, IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS = false)
        //{
        //    string newFileName = string.Empty;

        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            // Keep existing file if nothing new is uploaded
        //            return existingFilePath;
        //        }

        //        // Delete old file from current project if present
        //        if (!string.IsNullOrEmpty(existingFilePath))
        //        {
        //            string oldFilePath = Path.Combine(hostingEnvironment.WebRootPath, FileLocation.ImageUploadFolder, existingFilePath);
        //            if (System.IO.File.Exists(oldFilePath))
        //            {
        //                System.IO.File.Delete(oldFilePath);
        //            }
        //        }

        //        // Generate unique file name
        //        string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
        //        string fileExtension = Path.GetExtension(file.FileName);
        //        newFileName = $"{myUniqueFileName}{fileExtension}";

        //        // Decide upload folder (image/doc)
        //        string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
        //            fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //            fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //            fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //            fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
        //                ? FileLocation.ImageUploadFolder
        //                : FileLocation.DocumentUploadFolder);

        //        Directory.CreateDirectory(uploadFolder);

        //        // Use MemoryStream so content can be reused
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            // 1️⃣ Copy file to memory once
        //            await file.CopyToAsync(memoryStream);

        //            // 2️⃣ Save in current project
        //            memoryStream.Position = 0;
        //            string filePath = Path.Combine(uploadFolder, newFileName);
        //            using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //            {
        //                await memoryStream.CopyToAsync(fs);
        //            }

        //            // 3️⃣ Save in AMS project (optional)
        //            if (isToUploadInAMS)
        //            {
        //                bool isImageType =
        //                    fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //                    fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //                    fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //                    fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);

        //                string AmsApiUrl = Common.ImageAndDocSavePageAddress() +
        //                                         (isImageType ? "Files/UploadImage" : "Files/UploadDocument");

        //                memoryStream.Position = 0;

        //                using (var client = new HttpClient())
        //                using (var content = new MultipartFormDataContent())
        //                {
        //                    var streamContent = new StreamContent(memoryStream);
        //                    streamContent.Headers.Add("Content-Type", file.ContentType);

        //                    content.Add(streamContent, name: "file", fileName: newFileName);

        //                    HttpResponseMessage response = await client.PostAsync(AmsApiUrl, content);

        //                    if (!response.IsSuccessStatusCode)
        //                    {
        //                        // Optionally log response or handle retries
        //                        return string.Empty;
        //                    }
        //                }
        //            }
        //        }

        //        return newFileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception here if needed
        //        return string.Empty;
        //    }
        //}




        //    public string UploadFile(IFormFile file, string existingFilePath, IWebHostEnvironment hostingEnvironment, bool isToUploadInAMS = false)
        //    {
        //        try
        //        {
        //            string newFileName = string.Empty;

        //            if (file != null)
        //            {
        //                // Delete old file from current project
        //                if (!string.IsNullOrEmpty(existingFilePath))
        //                {
        //                    string oldFilePath = Path.Combine(hostingEnvironment.WebRootPath, FileLocation.ImageUploadFolder, existingFilePath);
        //                    if (System.IO.File.Exists(oldFilePath))
        //                    {
        //                        System.IO.File.Delete(oldFilePath);
        //                    }
        //                }

        //                if (file.Length > 0)
        //                {
        //                    // Generate unique filename
        //                    string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
        //                    string fileExtension = Path.GetExtension(file.FileName);
        //                    newFileName = $"{myUniqueFileName}{fileExtension}";

        //                    // Save in current project
        //                    //string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, FileLocation.ImageUploadFolder);
        //                    string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
        //fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
        //    ? FileLocation.ImageUploadFolder
        //    : FileLocation.DocumentUploadFolder);
        //                    Directory.CreateDirectory(uploadFolder);
        //                    string filePath = Path.Combine(uploadFolder, newFileName);

        //                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //                    {
        //                        file.CopyTo(fs);
        //                    }

        //                    // Save in another project if required
        //                    if (isToUploadInAMS)
        //                    {
        //                        string uploadFolderInAMS = @"D:\HCM\cloudtree24\AMS_Frontend\AdminPanel\wwwroot\assets\UploadImg";
        //                        Directory.CreateDirectory(uploadFolderInAMS);
        //                        string amsFilePath = Path.Combine(uploadFolderInAMS, newFileName);

        //                        using (FileStream fs = new FileStream(amsFilePath, FileMode.Create))
        //                        {
        //                            file.CopyTo(fs);
        //                        }
        //                    }
        //                    if (isToUploadInAMS)
        //                    {
        //                        bool isImageType = fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //    fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //    fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        //    fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
        //                        string hcmPortalApiUrl = Common.ImageAndDocSavePageAddress() + ((isImageType) ? "Files/UploadImage" : "Files/UploadDocument");

        //                        memoryStream.Position = 0;

        //                        using (var client = new HttpClient())
        //                        using (var content = new MultipartFormDataContent())
        //                        {
        //                            var streamContent = new StreamContent(memoryStream);
        //                            streamContent.Headers.Add("Content-Type", file.ContentType);

        //                            content.Add(streamContent, name: "file", fileName: newFileName);

        //                            HttpResponseMessage response = await client.PostAsync(hcmPortalApiUrl, content);

        //                            if (!response.IsSuccessStatusCode)
        //                            {
        //                                return String.Empty;
        //                            }
        //                        }
        //                    }





        //                }
        //            }
        //            else
        //            {
        //                // Keep the old file if nothing new is uploaded
        //                newFileName = existingFilePath;
        //            }

        //            return newFileName;
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }




        public async Task<int> GetLastTicketid()
        {
            int LastRecordId = 0;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response =await httpClient.GetAsync(Common.GetBaseAddress() + "SupportManagement/GetLastTicketid"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse =await response.Content.ReadAsStringAsync();
                        LastRecordId = JsonConvert.DeserializeObject<int>(apiResponse);
                    }
                }
            }
            return LastRecordId;
        }
        public string UploadFile(IFormFile file, IWebHostEnvironment hostingEnvironment)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return string.Empty;

                // Generate unique file name
                string myUniqueFileName = DateTime.Now.ToString("ddMMyyyyHHmmss");
                string fileExtension = Path.GetExtension(file.FileName);
                string newFileName = $"{myUniqueFileName}{fileExtension}";

                // AdminPanel upload folder
                //string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, "assets/UploadImg");
                string uploadFolder = Path.Combine(hostingEnvironment.WebRootPath,
    fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
    fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
    fileExtension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
    fileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
        ? FileLocation.ImageUploadFolder
        : FileLocation.DocumentUploadFolder);
                Directory.CreateDirectory(uploadFolder);

               

                // Save file in AdminPanel
                string adminFilePath = Path.Combine(uploadFolder, newFileName);
                using (FileStream fs = new FileStream(adminFilePath, FileMode.Create))
                {
                    file.CopyTo(fs);
                }
                return newFileName;
            }
            catch
            {
                return string.Empty;
            }
        }
       
        public async Task< PortalCredentialViewModel> GetPortalCustomerdetails(int csid)
        {
            
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();
            if (csid>0)
            {
                using (var httpClient = Common.GetHttpClient())
                {



                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/GetHCMportalcustomerdetails/" + csid))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            portalCredentialViewModel = JsonConvert.DeserializeObject<PortalCredentialViewModel>(apiResponse)??new PortalCredentialViewModel();

                        }
                    }
                }
            }
            return portalCredentialViewModel;
        }
        public async Task<TblTaxMaster> getTaxdetails()
        {
            TblTaxMaster tblTaxMaster = new TblTaxMaster(); 
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "TaxMaster/CheckExistingTax"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = response.Content.ReadAsStringAsync().Result;
                        tblTaxMaster = JsonConvert.DeserializeObject<TblTaxMaster>(apiResponse) ?? new TblTaxMaster();
                    }
                }
            }
            return tblTaxMaster;
        }
        public async Task<List<PlanMaster>> getAllPlans()
        {
            
            List<PlanMaster>? Plans = new List<PlanMaster>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSPlanMaster/GetAllPlans"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        Plans = JsonConvert.DeserializeObject<List<PlanMaster>>(apiResponse);
                        Plans = (Plans != null) ? Plans.OrderByDescending(e => e.PlanId).ToList() : Plans;
                        
                    }
                }
            }
            return Plans;
        }
        public  List<TblLeadSourceMaster> GetAllSource()
        {
            List<TblLeadSourceMaster> tblLeadSourceMasters = new List<TblLeadSourceMaster>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GetAllSource").Result)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            tblLeadSourceMasters = JsonConvert.DeserializeObject<List<TblLeadSourceMaster>>(apiResponse) ?? new List<TblLeadSourceMaster>();
                        }
                    }
                }
                return tblLeadSourceMasters;
            }
            catch
            {
                return tblLeadSourceMasters;
            }
        }


        public async Task<List<SubscriptionDetailsViewModel>> GetAllSubscriptionMaster()
        {
            List<SubscriptionDetailsViewModel> subscriptions = new List<SubscriptionDetailsViewModel>();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetSubscriptionMasterDetailsList"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            subscriptions = JsonConvert.DeserializeObject<List<SubscriptionDetailsViewModel>>(apiResponse) ?? new List<SubscriptionDetailsViewModel>();
                        }
                    }
                }
                return subscriptions;
            }
            catch
            {
                return subscriptions;
            }
        }

        public async Task<List<PlanMaster>> GetAllPlanMaster()
        {
            List<PlanMaster> plans = new List<PlanMaster>();
            try
            {
                List<PlanMaster>? Plans = new List<PlanMaster>();
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSPlanMaster/GetAllPlans"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            Plans = JsonConvert.DeserializeObject<List<PlanMaster>>(apiResponse);
                            Plans = (Plans != null) ? Plans.OrderByDescending(e => e.PlanId).ToList() : Plans;
                        }
                    }
                }
                return plans;
            }
            catch
            {
                return plans;
            }
        }


        public async Task<int> getdiscountUsagecount(int dmid,  int userid)
        {
            int usagecount = 0;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Discount/getUsageCount/" + dmid+"/"+userid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        usagecount = JsonConvert.DeserializeObject<int>(apiResponse);

                    }
                }
            }
            return usagecount;
        }

        public async Task<string> getLatestBrouchure()
        {
           string brochureName = string.Empty;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetBrochure" ))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        brochureName = await response.Content.ReadAsStringAsync();
                        //brochureName = JsonConvert.DeserializeObject<string>(apiResponse)??string.Empty;
                       
                    }
                }
            }
            return brochureName.Trim();
        }
        //to send the notification---Jayanta(29-11-2025)
        public async Task<ResponseModel> SaveHCMPortalNotification(TblCrmcommunicationTransaction notification)
        {
            ResponseModel responseModel = new ResponseModel();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(notification), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "AMSCustomerLedger/SendNewMessage", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }
                }
                return responseModel;
            }
            catch
            {
                return responseModel;
            }
        }

        public async Task<SelfMasterDetailsViewModel> GetSelfMasterDetails()
        {
            SelfMasterDetailsViewModel masterDetailsViewModel = new SelfMasterDetailsViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSSelfDetailsMaster/GetSelfDetailsMaster"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        masterDetailsViewModel = JsonConvert.DeserializeObject<SelfMasterDetailsViewModel>(apiResponse) ?? new SelfMasterDetailsViewModel();
                    }
                }
            }
            return masterDetailsViewModel;
        }

        public async Task<ResponseModel> deleterequestStatusActive(int csid)
        {
            ResponseModel responseModel = new ResponseModel();
            List<AccountDeleteViewModel> tblAccountDelete = new();

            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() +
                               "CRMCustomerManagement/GetAllRequestOfAccountDelete");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    tblAccountDelete = JsonConvert.DeserializeObject<List<AccountDeleteViewModel>>(apiResponse)
                                        ?? new List<AccountDeleteViewModel>();
                }
                bool exists = tblAccountDelete.Any(e => e.CustomerId == csid);

                responseModel.IsExist = exists;
                // tblAccountDelete= tblAccountDelete.Where(e=>e.CustomerId==csid).ToList();
            }
            return responseModel;
        }
        public async Task<ResponseModel> CheckCustomerSubscriptionExists(int csid)
        {
            ResponseModel responseModel = new ResponseModel();
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() +
                               "CRMCustomerManagement/CheckCustomerSubscriptionExists/"+csid);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse)
                                        ?? new ResponseModel();
                }               
               
            }
            return responseModel;
        }
        public async Task<ResponseModel> StopMultipleSubscription(int csid)
        {
            ResponseModel responseModel = new ResponseModel();
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() +
                               "OrderMaster/StopMultipleSubscription/" + csid);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse)
                                        ?? new ResponseModel();
                }

            }
            return responseModel;
        }

    }
}
