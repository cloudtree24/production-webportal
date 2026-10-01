using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Core;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace HCMPortal.Controllers
{
    public class DashboardController : Controller
    {

        private CommonServices? commonService;

        private readonly IWebHostEnvironment hostingenvironment;
        public DashboardController(IWebHostEnvironment _hostingenvironment)
        {
            hostingenvironment = _hostingenvironment;
        }

        /// <summary>
        /// this is the landing page 
        /// </summary>
        /// <returns></returns>
        /// 
        //[HttpGet("HCM-Solution/Product", Name = "HCMProduct")]
        //// for URL optimize--Anshuman 13.07.2026
        //public async Task<IActionResult> Index()
        //{
        //    SubscriptionPageViewModel suscriptionPageViewModel = new SubscriptionPageViewModel();
        //    List<WebBanner> bannerList = new List<WebBanner>();
        //    using (var httpClient = Common.GetHttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetAllActiveBanner"))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string apiResponse = await response.Content.ReadAsStringAsync();
        //                bannerList = JsonConvert.DeserializeObject<List<WebBanner>>(apiResponse) ?? new List<WebBanner>();
        //            }
        //        }
        //    }
        //    foreach(var item in bannerList)
        //    {
        //        if (String.IsNullOrEmpty(item.ImageName))
        //        {
        //            item.ImageName = "/assets/images/user_icon.png";
        //        }
        //        else
        //        {
        //            // 1. Define the relative path within the wwwroot folder
        //            string relativePath = Path.Combine("assets", "UploadImg", item.ImageName);

        //            // 2. Get the full physical path on the server
        //            // The WebRootPath points to the application's wwwroot folder.
        //            string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);

        //            // 3. Check if the file exists
        //            bool fileExists = System.IO.File.Exists(absolutePath);

        //            if (fileExists)
        //            {
        //                // The file is present
        //                item.ImageName = $"/{relativePath}";
        //            }
        //            else
        //            {
        //                // The file is not present
        //                item.ImageName = Common.ImageAndDocSavePageAddress() + "assets/UploadImg/" + item.ImageName;
        //            }
        //        }
        //    }
        //    // ====================== TESTIMONIAL LIST DAITARI(25-11-2025) ======================
        //    List<TestimonialModel> testimonialList = new List<TestimonialModel>();
        //    using (var httpClient = Common.GetHttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Testimonial/GetAllTestimonialList"))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string apiResponse = await response.Content.ReadAsStringAsync();
        //                testimonialList = JsonConvert.DeserializeObject<List<TestimonialModel>>(apiResponse) ?? new List<TestimonialModel>();
        //            }
        //        }
        //    }
        //    foreach (var item in testimonialList)
        //    {
        //        if (String.IsNullOrEmpty(item.pimage))
        //        {
        //            item.pimage = Common.ImageAndDocSavePageAddress().Replace("/api/", "/") + "assets/UploadImg/27102025153016.png";
        //        }
        //        else
        //        {
        //            string relativePath = Path.Combine("assets", "UploadImg", item.pimage);
        //            string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);
        //            bool fileExists = System.IO.File.Exists(absolutePath);
        //            if (fileExists)
        //            {
        //                item.pimage = $"/{relativePath}";
        //            }
        //            else
        //            {
        //                item.pimage = Common.ImageAndDocSavePageAddress().Replace("/api/", "/") + "assets/UploadImg/" + item.pimage;
        //            }
        //        }
        //    }
        //    suscriptionPageViewModel.testimonialList = testimonialList;
        //    suscriptionPageViewModel.bannerList = bannerList;





        //    List<TblHomeFeatureMaster> featureList = new List<TblHomeFeatureMaster>();
        //    using (var httpClient = Common.GetHttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "HomeFeature/GetAllFeatures"))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string apiResponse = await response.Content.ReadAsStringAsync();
        //                featureList = JsonConvert.DeserializeObject<List<TblHomeFeatureMaster>>(apiResponse) ?? new List<TblHomeFeatureMaster>();
        //            }
        //        }
        //    }
        //    suscriptionPageViewModel.featureList = featureList;
        //    //ViewBag.Exception = TempData["Exception"];
        //    ViewBag.Success = TempData["Success"];
        //    TempData["Success"]=null;
        //    return View(suscriptionPageViewModel);

        //}
        [HttpGet("hcm-solution", Name = "HCMProduct")]
        public async Task<IActionResult> Index()
        {
            var subscriptionPageViewModel = new SubscriptionPageViewModel();

            // Use 'using' block to ensure the client is disposed of correctly
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using (var client = Common.GetHttpClient())
            {
               
                // 1. Initiate API requests in parallel
                var bannerTask = client.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetAllActiveBanner");
                var testimonialTask = client.GetAsync(Common.GetBaseAddress() + "Testimonial/GetAllTestimonialList");
                var featureTask = client.GetAsync(Common.GetBaseAddress() + "HomeFeature/GetAllFeatures");

                // 2. Wait for all requests to finish simultaneously
                await Task.WhenAll(bannerTask, testimonialTask, featureTask);
                sw.Stop();
                System.Diagnostics.Debug.WriteLine($"Total API time: {sw.ElapsedMilliseconds}ms");
                // Log individual times to your debug output
                System.Diagnostics.Debug.WriteLine($"Banner: {bannerTask.IsCompletedSuccessfully}, Testimonial: {testimonialTask.IsCompletedSuccessfully}, Feature: {featureTask.IsCompletedSuccessfully}");
                //sw.Reset();
                sw.Start();
                // 3. Process Banner List
                if (bannerTask.Result.IsSuccessStatusCode)
                {
                    var json = await bannerTask.Result.Content.ReadAsStringAsync();
                    var bannerList = JsonConvert.DeserializeObject<List<WebBanner>>(json) ?? new List<WebBanner>();
                    foreach (var item in bannerList)
                    {
                        item.ImageName = ProcessImagePath(item.ImageName, true);
                    }
                    subscriptionPageViewModel.bannerList = bannerList;
                }

                // 4. Process Testimonial List
                if (testimonialTask.Result.IsSuccessStatusCode)
                {
                    var json = await testimonialTask.Result.Content.ReadAsStringAsync();
                    var testimonialList = JsonConvert.DeserializeObject<List<TestimonialModel>>(json) ?? new List<TestimonialModel>();
                    foreach (var item in testimonialList)
                    {
                        item.pimage = ProcessImagePath(item.pimage, false);
                    }
                    subscriptionPageViewModel.testimonialList = testimonialList;
                }

                // 5. Process Feature List
                if (featureTask.Result.IsSuccessStatusCode)
                {
                    var json = await featureTask.Result.Content.ReadAsStringAsync();
                    subscriptionPageViewModel.featureList = JsonConvert.DeserializeObject<List<TblHomeFeatureMaster>>(json) ?? new List<TblHomeFeatureMaster>();
                }
            } // HttpClient is automatically disposed of here

            ViewBag.Success = TempData["Success"];
            TempData["Success"] = null;
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"Processing finished at: {sw.ElapsedMilliseconds}ms");
            return View(subscriptionPageViewModel);
        }

        private string ProcessImagePath(string imageName, bool isBanner)
        {
            string defaultImg = isBanner ? "/assets/images/user_icon.png" : Common.ImageAndDocSavePageAddress().Replace("/api/", "/") + "assets/UploadImg/27102025153016.png";

            if (string.IsNullOrEmpty(imageName)) return defaultImg;

            string relativePath = Path.Combine("assets", "UploadImg", imageName);
            string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                return $"/{relativePath}";
            }

            return Common.ImageAndDocSavePageAddress() + "assets/UploadImg/" + imageName;
        }
        private async Task<bool> VerifyRecaptcha(string gRecaptchaResponse)
        {
            var secretKey = "6LcP1r4rAAAAAAUqp1ZfJeEqnAEJlG1m5FSwJnjF"; // ⚠️ from Google Admin Console

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={gRecaptchaResponse}",
                null
            );

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            return result?.success == true;
        }
        public async Task<IActionResult> DownloadBrochure(string file)
        {
            try
            {
                if (string.IsNullOrEmpty(file))
                    return NotFound("File name missing.");

                string baseUrl = Common.GetUrlImageAndDocAddress().TrimEnd('/');

                // Construct the EXACT FILE PATH
                string fileUrl = $"{baseUrl}/assets/UploadDoc/{file}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(fileUrl);

                    if (!response.IsSuccessStatusCode)
                        return NotFound("Remote file not found.");

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // IF <= 10 KB → HTML error page, NOT PDF
                    if (fileBytes.Length < 10000)
                    {
                        string html = await response.Content.ReadAsStringAsync();
                        System.IO.File.WriteAllText("C:/temp/debug.html", html);
                        return NotFound("Downloaded file is not a valid PDF. Check debug.html.");
                    }

                    return File(fileBytes, "application/pdf", file);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetContentType(string file)
        {
            var ext = Path.GetExtension(file).ToLower();

            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream",
            };
        }


        [HttpPost]
        public async Task<IActionResult> SendBrochureLink(string FullName, string Email, string Phone, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            try
            {
                if (!await VerifyRecaptcha(gRecaptchaResponse))
                    return Json(new { success = false, message = "Please validate the reCAPTCHA before submitting." });

                commonService = new CommonServices();
                string brochurename = await commonService.getLatestBrouchure();

                if (string.IsNullOrEmpty(brochurename))
                    return Json(new { success = false, message = "Brochure file not found." });

                // ✔ Use external app URL
                string appBBaseUrl = $"{Request.Scheme}://{Request.Host}";

                // ✔ Final external download URL
                string BrochureLink = $"{appBBaseUrl}/Dashboard/DownloadBrochure?file={brochurename}";

                string subject = "Download Brochure - HCM";

                //        string body = $@"
                //    <p>Hello,</p>
                //    <p>Please click the link below to download your brochure:</p>
                //    <p><a href='{BrochureLink}' target='_blank'>Download Brochure</a></p>
                //    <p>Thank you!</p>
                //";
                string body = $@"<!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Download Brochure</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        background-color: #f9f9f9;
                        margin: 0;
                        padding: 0;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: 20px auto;
                        background: #ffffff;
                        border: 1px solid #ddd;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    }}
                    .email-header {{
                        background: #007bff;
                        color: #ffffff;
                        padding: 20px;
                        text-align: center;
                    }}
                    .email-header h1 {{
                        margin: 0;
                        font-size: 24px;
                    }}
                    .email-body {{
                        padding: 20px;
                    }}
                    .email-body p {{
                        margin: 0 0 15px;
                    }}
                    .email-body a.button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: #ffffff !important;
                        padding: 10px 20px;
                        border-radius: 5px;
                        text-decoration: none;
                        font-weight: bold;
                    }}
                    .email-footer {{
                        background: #f4f4f4;
                        text-align: center;
                        padding: 15px;
                        font-size: 14px;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h1>Download HCM Brochure</h1>
                    </div>
                    <div class='email-body'>
                        <p>Dear {FullName},</p>
                        <p>Thank you for showing interest in our <strong>HCM</strong> — India’s most feature-rich HRMS platform.</p>
                        <p>Click the button below to download HCM brochure and explore how HCM can simplify your HR operations.</p>
                        <p style='text-align:center; margin: 25px 0;'>
                            <a href='{BrochureLink}' class='button' target='_blank'>Download Brochure</a>
                        </p>
                        <p>If the button doesn’t work, copy and paste the link below into your browser:</p>
                        <p><a href='{BrochureLink}'>{BrochureLink}</a></p>
                        <p>We hope this gives you deeper insights into how our solutions can benefit your organization.</p>
                        <p>For any queries, feel free to reach us at <a href='mailto:info@thecloudtree.ai'>info@thecloudtree.ai</a>.</p>
                        <p>Warm regards,<br>The Cloud Tree Team<br></p>
                    </div>
                    <div class='email-footer'>
                        <p>info@thecloudtree.ai | (+91) 9090973033</p>
                        <p>&copy; {DateTime.Now.Year} ThecloudTree. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";


                SendEmail(Email, subject, body);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            //            try
            //            {
            //                if (!await VerifyRecaptcha(gRecaptchaResponse))
            //                {
            //                    return Json(new { success = false, message = "Please validate the reCAPTCHA before submitting." });
            //                }
            //                commonService = new CommonServices();
            //                string brochurename = await commonService.getLatestBrouchure();
            //                string BrochureLink = $"{Common.GetUrlImageAndDocAddress()}assets/UploadDoc/{brochurename}";
            //                // 📨 Compose email
            //                string subject = "Download Brochure - HCM ";
            //                string body = $@"<!DOCTYPE html>
            //<html lang='en'>
            //<head>
            //    <meta charset='UTF-8'>
            //    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            //    <title>Download Brochure</title>
            //    <style>
            //        body {{
            //            font-family: Arial, sans-serif;
            //            line-height: 1.6;
            //            color: #333;
            //            background-color: #f9f9f9;
            //            margin: 0;
            //            padding: 0;
            //        }}
            //        .email-container {{
            //            max-width: 600px;
            //            margin: 20px auto;
            //            background: #ffffff;
            //            border: 1px solid #ddd;
            //            border-radius: 8px;
            //            overflow: hidden;
            //            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            //        }}
            //        .email-header {{
            //            background: #007bff;
            //            color: #ffffff;
            //            padding: 20px;
            //            text-align: center;
            //        }}
            //        .email-header h1 {{
            //            margin: 0;
            //            font-size: 24px;
            //        }}
            //        .email-body {{
            //            padding: 20px;
            //        }}
            //        .email-body p {{
            //            margin: 0 0 15px;
            //        }}
            //        .email-body a.button {{
            //            display: inline-block;
            //            background-color: #007bff;
            //            color: #ffffff !important;
            //            padding: 10px 20px;
            //            border-radius: 5px;
            //            text-decoration: none;
            //            font-weight: bold;
            //        }}
            //        .email-footer {{
            //            background: #f4f4f4;
            //            text-align: center;
            //            padding: 15px;
            //            font-size: 14px;
            //            color: #777;
            //        }}
            //    </style>
            //</head>
            //<body>
            //    <div class='email-container'>
            //        <div class='email-header'>
            //            <h1>Download HCM Brochure</h1>
            //        </div>
            //        <div class='email-body'>
            //            <p>Dear {FullName},</p>
            //            <p>Thank you for showing interest in our <strong>HCM</strong> — India’s most feature-rich HRMS platform.</p>
            //            <p>Click the button below to download HCM brochure and explore how HCM can simplify your HR operations.</p>
            //            <p style='text-align:center; margin: 25px 0;'>
            //                <a href='{BrochureLink}' download class='button' target='_blank'>Download Brochure</a>
            //            </p>
            //            <p>If the button doesn’t work, copy and paste the link below into your browser:</p>
            //            <p><a href='{BrochureLink}'>{BrochureLink}</a></p>
            //            <p>We hope this gives you deeper insights into how our solutions can benefit your organization.</p>
            //            <p>For any queries, feel free to reach us at <a href='mailto:info@thecloudtree.ai'>info@thecloudtree.ai</a>.</p>
            //            <p>Warm regards,<br>The Cloud Tree Team<br></p>
            //        </div>
            //        <div class='email-footer'>
            //            <p>info@thecloudtree.ai | (+91) 9090973033</p>
            //            <p>&copy; {DateTime.Now.Year} ThecloudTree. All rights reserved.</p>
            //        </div>
            //    </div>
            //</body>
            //</html>";



            //                // 🔧 Replace this with your actual email service
            //                SendEmail(Email, subject, body);

            //                return Json(new { success = true });
            //            }
            //            catch (Exception ex)
            //            {
            //                // Log error
            //                return Json(new { success = false, message = ex.Message });
            //            }
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            commonService=new CommonServices();
            string fromEmail = "thecloudtree.developer@gmail.com";
            string password = "jdndluquofjmwzjl";
            //EmailServerDetailsViewModel econfig = new EmailServerDetailsViewModel();
            //econfig = commonService.GetEmailServerDetails(companyid).Result;
            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(fromEmail ?? String.Empty);
                msg.To.Add(toEmail);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                SmtpClient smt = new SmtpClient();
                smt.Host = "smtp.gmail.com" ?? String.Empty;
                System.Net.NetworkCredential ntwd = new NetworkCredential();
                ntwd.UserName = fromEmail;
                ntwd.Password =password;
                smt.UseDefaultCredentials = false;
                smt.Credentials = ntwd;
                smt.Port = 587;
                smt.DeliveryMethod = SmtpDeliveryMethod.Network;
                smt.EnableSsl = true;
                smt.Timeout = 300000000;
                smt.Send(msg);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
