//using AdminPanel.Models;
using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Generators;
using Razorpay.Api;
using System.ComponentModel.Design;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;


namespace HCMPortal.Controllers
{
    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

    public class CustomerManagementController : Controller
    {
        ILogger<CustomerManagementController> logger;
        private readonly IWebHostEnvironment hostingenvironment;
        private CommonServices? commonService;
        //private readonly UserManager<IdentityUser> _userManager;
        // to implement the gstin validation--Jayanta(21-01-2026)
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public CustomerManagementController(ILogger<CustomerManagementController> _logger, IHostingEnvironment hostingenvironment)
        {
            logger = _logger;
            this.hostingenvironment = hostingenvironment;
            //_userManager = userManager;
        }
        public IActionResult CustomerManagement()
        {
            return View();
        }
        //[HttpGet]
        // public IActionResult RegisterHCMPortalCustomer()


        [HttpGet("customer/register", Name = "CustomerRegister")]
        // for URL optimize--Anshuman 13.07.2026
        public IActionResult RegisterHCMPortalCustomer()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            //Handel Exception Daitari(25-11-2025)
            try
            {
                PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();
                commonService = new CommonServices();
                ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");
                portalCredentialViewModel.CountryId =commonService.CountryList().Where(e => e.CountryName == "India").Select(e => e.CountryId).FirstOrDefault();
                ViewBag.StateList = commonService.StateList();
                ViewBag.IndusryType = commonService.IndustyList();
                ViewBag.Success = TempData["Success"];
                ViewBag.Exception = TempData["Exception"];
                TempData["Success"] = null;
                TempData["Exception"] = null;
                return View(portalCredentialViewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("customer/register")]
        public async Task<IActionResult> RegisterHCMPortalCustomer(PortalCredentialViewModel portalCredentialViewModel, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            //using var httpClient = new HttpClient();

            commonService = new CommonServices();
            // ViewBag.CountryList = commonService.CountryList();
            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");

            ViewBag.StateList = commonService.StateList();
            ViewBag.IndusryType = commonService.IndustyList();
            ResponseModel responseModel = new ResponseModel();
            portalCredentialViewModel.AccountStatus = "Active";
            if (!await VerifyRecaptcha(gRecaptchaResponse))
            {
                ModelState.AddModelError("", "reCAPTCHA validation failed.");
                return View(portalCredentialViewModel);
            }
            try
            {
                if (ModelState.IsValid)
                {
                    string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    var random = new Random();
                    string password = new string(Enumerable.Range(1, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
                    if (portalCredentialViewModel.Csid == 0)
                    {
                        portalCredentialViewModel.PortalPassword = password;
                    }

                    using (var httpClient = Common.GetHttpClient())
                    {

                        StringContent stringContent = new StringContent(JsonConvert.SerializeObject(portalCredentialViewModel), Encoding.UTF8, "application/json");

                        using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/CreateUpdateCRMCustomer", stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                            }
                        }
                    }

                    if (responseModel.IsSuccess == true)
                    {
                        string ownbaseaddress = Common.GetUrlForOwnBaseAddress();
                        string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    " +
                            "<meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n " +
                            "   <title>Welcome Email</title>\r\n " +
                            "   <style>\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            line-height: 1.6;\r\n            color: #333;\r\n            background-color: #f9f9f9;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n        .email-container {\r\n            max-width: 600px;\r\n            margin: 20px auto;\r\n            background: #ffffff;\r\n            border: 1px solid #ddd;\r\n            border-radius: 8px;\r\n            overflow: hidden;\r\n            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\r\n        }\r\n        .email-header {\r\n            background: #007bff;\r\n            color: #ffffff;\r\n            padding: 20px;\r\n            text-align: center;\r\n        }\r\n        .email-header h1 {\r\n            margin: 0;\r\n            font-size: 24px;\r\n        }\r\n        .email-body {\r\n            padding: 20px;\r\n        }\r\n        .email-body p {\r\n            margin: 0 0 15px;\r\n        }\r\n        .email-body a {\r\n            color: #007bff;\r\n            text-decoration: none;\r\n        }\r\n        .email-footer {\r\n            background: #f4f4f4;\r\n            text-align: center;\r\n            padding: 15px;\r\n            font-size: 14px;\r\n            color: #777;\r\n        }\r\n    </style>\r\n" +
                            "</head>\r\n" +
                            "<body>\r\n   " +
                            " <div class=\"email-container\">\r\n        <div class=\"email-header\">\r\n            <h1>Welcome to HCM Portal!</h1>\r\n        </div>\r\n  " +
                            "      <div class=\"email-body\">\r\n            <p>Dear " + portalCredentialViewModel.ContactPersonName + ",</p>\r\n           " +
                            " <p>Thank you for registering with " +
                            "<strong>HCM Portal</strong>! We’re thrilled to have you as part of our community.</p>\r\n     " +
                            "       <p><strong>Here are your account details:</strong></p>\r\n     " +
                            "       <ul>\r\n               " +
                            " <li><strong>Portal Username:</strong> " + portalCredentialViewModel.EmailAddress + "</li>\r\n          " +
                            "      <li><strong>Portal Password:</strong> " + password + "</li>\r\n        " +
                            "    </ul>\r\n    " +
                            //"        <p>You can log in to your account anytime at <a href=\"https://hcm.anantagroup.in.net/\">https://hcm.anantagroup.in.net/</a>.</p>\r\n        " +
                            "        <p>You can log in to your account anytime at <a href=\"" + ownbaseaddress+"CustomerManagement/Login\"> "+ownbaseaddress+"CustomerManagement/Login/</a>.</p>\r\n        " +
                            "    <p><strong>Next Steps:</strong></p>\r\n            <ol>\r\n     " +
                            "           <li>Log in to your account using the credentials above.</li>\r\n " +
                            "       <li>Start exploring the Portal to get the most out of your account.</li>\r\n            </ol>\r\n   " +
                            "         <p>If you have any questions or need help, our support team is just a message away at <a href=\"mailto:info@thecloudtree.ai\">info@thecloudtree.ai</a>.</p>\r\n       " +
                            "     <p>Welcome aboard!</p>\r\n        </div>\r\n     " +
                            "   <div class=\"email-footer\">\r\n      " +
                            "      <p>Best regards,<br>thecloudTree.ai<br>HCM Portal</p>\r\n      " +
                            "      <p>info@thecloudtree.ai Ph : (+91)  9090973033</p>\r\n     " +
                            "   </div>\r\n    </div>\r\n</body>\r\n</html>\r\n";
                        if (!string.IsNullOrWhiteSpace(portalCredentialViewModel.EmailAddress))
                        {
                            SendEmail(portalCredentialViewModel.EmailAddress, "Welcome to HCM Portal! Let’s Begin Your Journey", body);

                            //if (TempData.ContainsKey("ReturnUrl"))
                            //{
                            //    string returnUrl = TempData["ReturnUrl"] as string;

                            //    if (!string.IsNullOrEmpty(returnUrl))
                            //    {
                            //        return Redirect(returnUrl);
                            //    }
                            //}
                        }
                        //SendEmail(portalCredentialViewModel.EmailAddress, "New Registration", body);
                        TempData["Success"] = "Account created successfully—login credentials have been sent to your registered email.";
                        //return View("GetLogin");
                        ModelState.Clear();
                        return RedirectToAction("Login", "CustomerManagement");
                        // return RedirectToAction(nameof(CustomerManagementList));
                    }
                    else
                    {
                        TempData["Exception"] = "Something went wrong!Pleased Try again after a while.";
                        return View();
                        //return View("CustomerManagementMaster", tblAmscustomer);
                    }

                }
                else
                {
                    TempData["Exception"] = "Enter All Required data";
                    ModelState.AddModelError("", "Field validation failed.");
                }

                // ViewBag.Success = TempData["Success"];
                //ViewBag.Exception = TempData["Exception"];
            }
            catch (Exception)
            {
                TempData["Exception"] = "Something went wrong!Pleased Try again after a while.";
            }

            return View(portalCredentialViewModel);
        }


        //public async Task<GSTResponseViewModel> ValidateGSTINAsync(string gstin)
        //{
        //    var result = new GSTResponseViewModel();

        //    var client = _httpClientFactory.CreateClient();

        //    client.DefaultRequestHeaders.Add(
        //        "X-RapidAPI-Key",
        //        _configuration["RapidAPI:Key"]
        //    );

        //    client.DefaultRequestHeaders.Add(
        //        "X-RapidAPI-Host",
        //        _configuration["RapidAPI:Host"]
        //    );

        //    var url = $"https://gst-details.p.rapidapi.com/gst/{gstin}";

        //    var response = await client.GetAsync(url);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        result.IsValid = false;
        //        result.Message = "Invalid GSTIN";
        //        return result;
        //    }

        //    var json = await response.Content.ReadAsStringAsync();
        //    dynamic data = JsonConvert.DeserializeObject(json);

        //    result.IsValid = true;
        //    result.GSTIN = data?.gstin;
        //    result.LegalName = data?.legal_name;
        //    result.TradeName = data?.trade_name;
        //    result.Status = data?.status;

        //    return result;
        //}
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

        private void SendEmail(string toEmail, string subject, string body)
        {
            commonService = new CommonServices();
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
                ntwd.Password = password;
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
        [HttpGet]
        public List<State> GetStateList(int countryId)
        {
            commonService = new CommonServices();
            return commonService.StateList(countryId);
        }

        public JsonResult IsEmailExist(PortalCredentialViewModel details)
        {
            ResponseModel responseModel = new ResponseModel();
            commonService = new CommonServices();
            try
            {
                //responseModel= commonService.IsEmailExist(PrimaryEmail, OldEmail);
                responseModel = commonService.IsEmailExist(details.EmailAddress, details.OldEmail);
                return Json(!responseModel?.IsExist);
            }
            catch (Exception)
            {
                return Json(!responseModel?.IsExist);
            }
        }
        public JsonResult IsMobileNoExist(PortalCredentialViewModel details)
        {
            ResponseModel responseModel = new ResponseModel();
            commonService = new CommonServices();
            try
            {
                //responseModel= commonService.IsEmailExist(PrimaryEmail, OldEmail);
                responseModel = commonService.IsMobileExist(details.Phonenumber, details.OldPhone);
                return Json(!responseModel?.IsExist);
            }
            catch (Exception)
            {
                return Json(!responseModel?.IsExist);
            }
        }
        [HttpGet("customer/login",Name ="CustomerLogin")]
        // for URL optimize--Anshuman 13.07.2026
        public IActionResult Login()
        {
            ViewBag.Success = TempData["Success"];
            ViewBag.Exception = TempData["Error"];
            TempData["Success"] = null;
            TempData["Error"] = null;
            // ViewBag.Success = TempData["Success"]; // read from TempData
            //ViewBag.Exception = TempData["Exception"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PCustomerLogin(LoginViewModel loginViewModel, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            ResponseModel responseModel = new ResponseModel();
            PortalCredential portalCredential = new PortalCredential();
            commonService = new CommonServices();

            //if (!await VerifyRecaptcha(gRecaptchaResponse))
            //{
            //    ModelState.AddModelError("", "reCAPTCHA validation failed.");
            //    return View("Login");
            //}

            //if (ModelState.IsValid)
            //{
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(loginViewModel), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/Login", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            portalCredential = JsonConvert.DeserializeObject<PortalCredential>(apiResponse) ?? new PortalCredential();

                            if (portalCredential.Csid != 0)
                            {
                                // Set session
                                HttpContext.Session.SetInt32("UserId", portalCredential.Csid);
                                HttpContext.Session.SetString("Contactperson", portalCredential.ContactPersonName ?? "NA");
                                HttpContext.Session.SetString("UserName", portalCredential.PortalUserName ?? "NA");
                                HttpContext.Session.SetString("MemberId", portalCredential.MembershipId ?? "NA");
                                HttpContext.Session.SetString("ProfileImage", portalCredential.CsProfileImage ?? "userIcon.webp");
                                HttpContext.Session.SetString("Emailaddress", portalCredential.EmailAddress ?? "NA");


                                // call a common service in which it will check iot has enterprise or not and set it into the session
                                bool isEnterprise = false;

                                using (var httpClients = Common.GetHttpClient())
                                {
                                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetEnterprisePlan?csid={portalCredential.Csid}";

                                    var responses = await httpClients.GetAsync(url);

                                    if (!response.IsSuccessStatusCode)
                                        return Content("Unable to fetch Enterprise Plan from DB.");

                                    dynamic result = JsonConvert.DeserializeObject(await responses.Content.ReadAsStringAsync()) ?? "";

                                    isEnterprise = result?.isEnterprise ?? false;
                                    HttpContext.Session.SetString("IsEnterprise", isEnterprise.ToString());
                                }

                                // Set TempData flash message
                                // TempData["Success"] = "You have Logged in successfully! Welcome to the HCM Portal";
                                //ViewBag.Success = TempData["Success"];
                                // TempData["Success"]=null;
                                // Redirect to ReturnUrl if exists
                                if (TempData["ReturnUrl"] != null)
                                {
                                    var returnUrl = TempData["ReturnUrl"].ToString();
                                    TempData.Remove("ReturnUrl");
                                    return Redirect(returnUrl);
                                }

                                // Correct: redirect to dashboard (not return view)
                                // return View("PortalDashboard");

                                //return RedirectToAction("PortalDashboard");
                                // for URL optimize--Anshuman 13.07.2026
                                return RedirectToAction("PortalDashboard");
                            }
                            else
                            {
                                TempData["Error"] = "Wrong credentials. Try again.";
                                return RedirectToAction("Login");
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Wrong credentials. Try again.";
                            return RedirectToAction("Login"); // <-- must **return**
                        }
                    }
                }
            //}

            return View("Login");
        }




        //[HttpGet]
        // public IActionResult PortalDashboard()
        // for URL optimize--Anshuman 13.07.2026
        [HttpGet("customer/dashboard",Name ="CustomerDashboard")]
        // for URL optimize--Anshuman 13.07.2026
        public async Task<IActionResult> PortalDashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Dashboard");
                //return RedirectToAction("Login");
            }
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            PortalDashboardViewModel portalDashboardView = new PortalDashboardViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
               
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetDashboardComponentDetails/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        portalDashboardView = JsonConvert.DeserializeObject<PortalDashboardViewModel>(apiResponse) ?? new PortalDashboardViewModel();
                        //portalDashboardView.isCompanySetupPending = true;
                    }
                    else
                    {
                        string error= await response.Content.ReadAsStringAsync();
                    }
                }
            }
             ViewBag.TrialStart = TempData["TrialStart"];
            portalDashboardView.EmpEmail = HttpContext.Session.GetString("Emailaddress") ?? string.Empty;
            return View(portalDashboardView);
        }
        //[HttpGet]
        [HttpGet("customer/logout", Name = "CustomerLogout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear session
            HttpContext.Session.Clear();

            // Prevent caching after logout
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            TempData["Success"] = "You have Logged Out Successfully!";
            TempData["ReturnUrl"] = null;
            return RedirectToAction("Index", "Dashboard");


        }
        [HttpGet("customer/forgot-password", Name = "CustomerForgotPassword")]
        public IActionResult PCustomerForgotPassword()
        {
            return View();
            ViewBag.Exception = TempData["Exception"];
            TempData["Exception"] = null;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerForgotPassword(string email)
        {

            ResponseModel responseModel = new ResponseModel();
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();
            using (var httpClient = Common.GetHttpClient())
            {

                //StringContent stringContent = new StringContent(JsonConvert.SerializeObject(forgotpasswordViewModel), Encoding.UTF8, "application/json");

                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/Forgotpassword/" + email))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        portalCredentialViewModel = JsonConvert.DeserializeObject<PortalCredentialViewModel>(apiResponse) ?? new PortalCredentialViewModel();

                    }
                }
            }
            if (portalCredentialViewModel.Csid > 0)
            {
                var expiry = DateTime.UtcNow.AddMinutes(120);
                var tokenData = $"{portalCredentialViewModel.EmailAddress}|{expiry}";
                var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
                var token = Convert.ToBase64String(tokenBytes);
                var encodedToken = WebUtility.UrlEncode(token);

                var resetLink = Url.Action("ResetPassword", "CustomerManagement",
    new { email = portalCredentialViewModel.EmailAddress, token = encodedToken },
    protocol: HttpContext.Request.Scheme);

                string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset</title>
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
        .email-body a.button {{
            display: inline-block;
            padding: 12px 20px;
            background: #007bff;
            color: #fff !important;
            text-decoration: none;
            border-radius: 5px;
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
   <div class=""email-container"">
        <div class=""email-header"">
            <h1>Password Reset Request</h1>
        </div>
        <div class=""email-body"">
            <p>Dear {portalCredentialViewModel.ContactPersonName},</p>
            <p>We received a request to reset your password for your <strong>HCM Portal</strong> account.</p>
            <p>Please click the button below to reset your password:</p>
            <p style=""text-align:center;"">
                <a href=""{resetLink}"" class=""button"">Reset My Password</a>
            </p>
            <p>If the button doesn’t work, copy and paste this link into your browser:</p>
            <p><a href=""{resetLink}"">{resetLink}</a></p>
            <p><strong>Note:</strong> This link will expire in 30 minutes for your security.</p>
            <p>If you did not request a password reset, please ignore this email.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM Portal</p>
            <p>info@thecloudtree.ai Ph : (+91)  9090973033</p>
        </div>
    </div>
</body>
</html>";

                SendEmail(portalCredentialViewModel.EmailAddress, "Reset password", body);
                responseModel.IsSuccess = true;
                responseModel.Message = "Reset Password link has been send to your email address";
            }
            else
            {
                // TempData["Exception"] = "Email address not found.";
                responseModel.IsSuccess = false;
                responseModel.Message = "Email Address Not Found, Please Try Again";

            }

            //return View("PCustomerForgotPassword");
            return Json(responseModel);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email,string token)
        {
            try
            {
                //string email = HttpContext.Session.GetString("Emailaddress")??string.Empty;
                // ✅ Decode the token
                var decodedBytes = Convert.FromBase64String(WebUtility.UrlDecode(token));
                var tokenData = Encoding.UTF8.GetString(decodedBytes);

                // ✅ Format: "email|expiry"
                var parts = tokenData.Split('|');
                if (parts.Length != 2)
                {
                    TempData["Error"] = "Invalid reset link.";
                    return RedirectToAction("Login", "CustomerManagement");
                }

                var tokenEmail = parts[0];
                var expiry = DateTime.Parse(parts[1]);

                // ✅ Check email matches
                if (!string.Equals(email, tokenEmail, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Invalid reset link.";
                    return RedirectToAction("Login", "CustomerManagement");
                }

                // ✅ Check expiry
                if (DateTime.UtcNow > expiry)
                {
                    TempData["Error"] = "This reset link has expired. Please request a new one.";
                    return RedirectToAction("ForgotPassword", "CustomerManagement");
                }

                // ✅ If valid, show ResetPassword form
                ResetPasswordViewModel model = new ResetPasswordViewModel
                {
                    Email = email,
                    Token = token
                };
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Invalid reset link.";
                return RedirectToAction("Login", "CustomerManagement");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var decodedBytes = Convert.FromBase64String(WebUtility.UrlDecode(model.Token));
                var tokenData = Encoding.UTF8.GetString(decodedBytes);

                var parts = tokenData.Split('|');
                if (parts.Length != 2) throw new Exception("Invalid token");

                var tokenEmail = parts[0];
                var expiry = DateTime.Parse(parts[1]);

                if (!string.Equals(model.Email, tokenEmail, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Invalid token");

                if (DateTime.UtcNow > expiry)
                    throw new Exception("Expired token");

                PortalCredential portalCredential = new PortalCredential();
                portalCredential.EmailAddress=model.Email;
                portalCredential.PortalPassword=model.NewPassword;
                // ✅ At this point, token is valid → reset password in DB
                // Your custom logic here...
                using (var httpClient = Common.GetHttpClient())
                {

                    //using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/ResetPassword/" + model.Email + " / " + model.NewPassword))
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(portalCredential), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/ProfileResetPassword", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                        }
                    }
                }
                if (responseModel.IsSuccess == true)
                {


                    string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Notification</title>
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
            background: #28a745;
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
   <div class=""email-container"">
        <div class=""email-header"">
            <h1>Password Reset Successful</h1>
        </div>
        <div class=""email-body"">
            <p>Dear Customer,</p>
            <p>This is to inform you that the password for your <strong>HCM Portal</strong> account was successfully reset recently.</p>
            <p>If you performed this action, no further steps are required.</p>
            <p>If you did <strong>not</strong> reset your password, please contact our support team immediately.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM Portal</p>
            <p>info@thecloudtree.ai &nbsp; | &nbsp; Ph: (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>";


                    SendEmail(model.Email ?? string.Empty, "Reset Password", body);
                    TempData["Success"] = "Password reset successfully.";
                    ViewBag.Success = TempData["Success"];
                    return RedirectToAction("Login", "CustomerManagement");
                }
                else
                {
                    TempData["Exception"] = "Error in resetting the password.";
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
        //[HttpGet]
        [HttpGet("customer/profile", Name = "CustomerProfile")]
        // for URL optimize--Anshuman 13.07.2026
        public async Task<IActionResult> PortalProfile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            commonService = new CommonServices();
            //ViewBag.CountryList = commonService.CountryList();
            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");

            ViewBag.StateList = commonService.StateList();
            ViewBag.IndusryType = commonService.IndustyList();
            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();
            if (csid > 0)
            {
                using (var httpClient = Common.GetHttpClient())
                {


                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/GetHCMportalcustomerdetails/" + csid))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            portalCredentialViewModel = JsonConvert.DeserializeObject<PortalCredentialViewModel>(apiResponse) ?? new PortalCredentialViewModel();

                        }
                        if (String.IsNullOrEmpty(portalCredentialViewModel.CsProfileImage))
                        {
                            portalCredentialViewModel.CsProfileImage = "/assets/users/user_icon.png";
                        }
                        else
                        {
                            // 1. Define the relative path within the wwwroot folder
                            string relativePath = Path.Combine("assets", "UploadImg", portalCredentialViewModel.CsProfileImage);

                            // 2. Get the full physical path on the server
                            // The WebRootPath points to the application's wwwroot folder.
                            string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);

                            // 3. Check if the file exists
                            bool fileExists = System.IO.File.Exists(absolutePath);

                            if (fileExists)
                            {
                                // The file is present
                                portalCredentialViewModel.CsProfileImage = $"/{relativePath}";
                            }
                            else
                            {
                                // The file is not present
                                portalCredentialViewModel.CsProfileImage = Common.ImageAndDocSavePageAddress() + "assets/UploadImg/" + portalCredentialViewModel.CsProfileImage;
                            }
                        }
                    }
                }
            }
            if (portalCredentialViewModel.Gstinnumber=="NA")
            {
                portalCredentialViewModel.Gstinnumber=string.Empty;
            }
            if (portalCredentialViewModel.Pancard=="NA")
            {
                portalCredentialViewModel.Pancard=string.Empty;
            }
            portalCredentialViewModel.OldEmail=portalCredentialViewModel.EmailAddress;
            portalCredentialViewModel.OldPhone=portalCredentialViewModel.Phonenumber;
            ViewBag.Success = TempData["Success"];
            ViewBag.Exception = TempData["Exception"];
            ResponseModel responseModel = new ResponseModel();
            responseModel=await commonService.deleterequestStatusActive(csid);
            ViewBag.Isexist=responseModel.IsExist;
            return View(portalCredentialViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePortalProfile(PortalCredentialViewModel portalCredentialViewModel)
        {
            commonService = new CommonServices();
            //ViewBag.CountryList = commonService.CountryList();
            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");

            ViewBag.StateList = commonService.StateList();
            ViewBag.IndusryType = commonService.IndustyList();
            ResponseModel responseModel = new ResponseModel();
            // portalCredentialViewModel.AccountStatus="Active";
            if (ModelState.IsValid)
            {
                if (portalCredentialViewModel.CsImageFile != null && portalCredentialViewModel.CsImageFile.Length > 0)
                    portalCredentialViewModel.CsProfileImage = await commonService.UploadFile(portalCredentialViewModel.CsImageFile, portalCredentialViewModel.CsProfileImage ?? String.Empty, hostingenvironment, true);
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(portalCredentialViewModel), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/CreateUpdateCRMCustomer", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                            //HttpContext.Session.SetString("ProfileImage", portalCredentialViewModel.CsProfileImage == null ? "userIcon.webp" : portalCredentialViewModel.CsProfileImage);
                            TempData["Success"] = "Profile updated success successfully.";
                            if (!string.IsNullOrWhiteSpace(portalCredentialViewModel.CsProfileImage))
                            {
                                HttpContext.Session.SetString(
                                    "ProfileImage",
                                    portalCredentialViewModel.CsProfileImage
                                );
                            }
                            //ViewBag.Success = TempData["Success"];
                        }
                        else
                        {

                            TempData["Exception"] = "Something went wrong in updation of the Profile Data"; 
                            ViewBag.Exception = TempData["Success"];
                            return View("PortalProfile");
                        }
                    }
                }
            }
            // int csid = HttpContext.Session.GetInt32("UserId") ?? 0;

            return RedirectToAction("PortalProfile");
        }


        [HttpPost]
        public IActionResult SendOtp()
        {
            string otp = new Random().Next(100000, 999999).ToString();
            string contactperson= HttpContext.Session.GetString("Contactperson")??string.Empty;

            HttpContext.Session.SetString("ProfileOTP", otp);

            string oldEmail = HttpContext.Session.GetString("Emailaddress")??string.Empty;
            string body = "<!DOCTYPE html>" +
"<html lang=\"en\">" +
"<head>" +
"    <meta charset=\"UTF-8\">" +
"    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
"    <title>OTP Verification</title>" +
"    <style>" +
"        body { font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0; }" +
"        .email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }" +
"        .email-header { background: #007bff; color: #ffffff; padding: 20px; text-align: center; }" +
"        .email-body { padding: 20px; color: #333; }" +
"        .otp-box { font-size: 26px; font-weight: bold; letter-spacing: 5px; text-align: center; background: #f4f4f4; padding: 15px; border-radius: 6px; margin: 20px 0; }" +
"        .email-footer { background: #f4f4f4; text-align: center; padding: 15px; font-size: 14px; color: #777; }" +
"    </style>" +
"</head>" +
"<body>" +
"    <div class=\"email-container\">" +
"        <div class=\"email-header\">" +
"            <h1>OTP Verification</h1>" +
"        </div>" +
"        <div class=\"email-body\">" +
"            <p>Dear " + contactperson + ",</p>" +
"            <p>We received a request to update your <strong>HCM Portal</strong> account details.</p>" +
"            <p>Please use the following One-Time Password (OTP) to verify your request:</p>" +
"            <div class=\"otp-box\">" + otp + "</div>" +
"            <p>This OTP is valid for <strong>5 minutes</strong>. Please do not share it with anyone.</p>" +
"            <p>If you did not request this change, please ignore this email or contact our support team immediately.</p>" +
"        </div>" +
"        <div class=\"email-footer\">" +
"            <p>Best regards,<br>thecloudTree.ai<br>HCM Portal</p>" +
"            <p>info@thecloudtree.ai | Ph: (+91) 9090973033</p>" +
"        </div>" +
"    </div>" +
"</body>" +
"</html>";

            SendEmail(oldEmail, "Welcome to HCM Portal! Let’s Begin Your Journey", body);
            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("ProfileOTP");

            if (otp == sessionOtp)
            {
                HttpContext.Session.Remove("ProfileOTP");
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public async Task<IActionResult> ValidateCurrentPassword(string currentPassword)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.PostAsync(
                    Common.GetBaseAddress() +
                    $"CRMCustomerManagement/ValidateCurrentPassword?userId={userId}&currentPassword={Uri.EscapeDataString(currentPassword)}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Content(result, "application/json");
                }
            }

            return Json(false);
        }

        //[HttpPost]
        //public async Task<IActionResult> ValidateCurrentPassword(string currentPassword)
        //{
        //    try
        //    {
        //        commonService=new CommonServices();
        //        PortalCredentialViewModel user = new PortalCredentialViewModel();
        //        int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
        //         user = await commonService.GetPortalCustomerdetails(csid);

        //        if (user == null || string.IsNullOrWhiteSpace(user.PortalPassword))
        //            return Json(false);

        //        bool isValid;

        //        if (IsBCryptHash(user.PortalPassword))
        //        {
        //            // ✅ BCrypt validation
        //            isValid = BCrypt.Net.BCrypt.Verify(
        //                currentPassword,
        //                user.PortalPassword
        //            );
        //        }
        //        else
        //        {
        //            // ⚠️ Legacy password (plain / encrypted)
        //            isValid = currentPassword.Trim()
        //                      .Equals(user.PortalPassword.Trim());
        //        }

        //        return Json(isValid);
        //    }
        //    catch
        //    {
        //        return Json(false);
        //    }
        //}

        private bool IsBCryptHash(string password)
        {
            return password.StartsWith("$2a$") ||
                   password.StartsWith("$2b$") ||
                   password.StartsWith("$2y$");
        }
        [HttpPost]
        //public async Task<IActionResult> ChangePortalpassword(string email, string newpassword)
        public async Task<IActionResult> ChangePortalpassword(string currentPassword, string newpassword)
        {
            commonService=new CommonServices();
            ResponseModel responseModel = new ResponseModel();
            if (!ModelState.IsValid)
                return RedirectToAction("PortalProfile");
           
            //if(currentPassword!=portalCredentialViewModel.PortalPassword)
            //{
            //    ModelState.AddModelError(string.Empty, "Current password is incorrect.");
            //    return RedirectToAction("PortalProfile");
            //}
            try
            {
               PortalCredential portalCredential = new PortalCredential();
                portalCredential.PortalPassword = newpassword;
                portalCredential.EmailAddress= HttpContext.Session.GetString("Emailaddress")??string.Empty;
                string email = HttpContext.Session.GetString("Emailaddress")??string.Empty;
                using (var httpClient = Common.GetHttpClient())
                {

                    //using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/ResetPassword/" + email + " / " + newpassword))
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(portalCredential), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CRMCustomerManagement/ProfileResetPassword", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                        }
                    }
                }
                if (responseModel.IsSuccess == true)
                {
                    string body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Notification</title>
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
            background: #28a745;
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
   <div class=""email-container"">
        <div class=""email-header"">
            <h1>Password Reset Successful</h1>
        </div>
        <div class=""email-body"">
            <p>Dear Customer,</p>
            <p>This is to inform you that the password for your <strong>HCM Portal</strong> account was successfully reset Password recently.</p>
            <p>If you performed this action, no further steps are required.</p>
            <p>If you did <strong>not</strong> reset your password, please contact our support team immediately.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM Portal</p>
            <p>info@thecloudtree.ai &nbsp; | &nbsp; Ph: (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>";


                    SendEmail(email, "Reset Password", body);
                    TempData["Success"] = "Password reset successfully.";
                    ViewBag.Success = TempData["Success"];
                    return RedirectToAction("Login", "CustomerManagement");
                }
                else
                {
                    TempData["Exception"] = "Error in resetting the password.";
                    return RedirectToAction("PortalProfile");
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("PortalProfile");
            }
        }


        [HttpPost]
      public async Task<IActionResult> DeleteAccount(string reason,[FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            commonService = new CommonServices();

            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (csid <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Session expired. Please login again."
                });
            }

            if (!await VerifyRecaptcha(gRecaptchaResponse))
            {
                return Json(new
                {
                    success = false,
                    message = "Please complete the reCAPTCHA before deleting your account."
                });
            }

            var deleteAccountDto = new DeleteAccountDto
            {
                Csid = csid,
                Reason = reason
            };

            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    var content = new StringContent(
                        JsonConvert.SerializeObject(deleteAccountDto),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await httpClient.PostAsync(
                        Common.GetBaseAddress() + "CRMCustomerManagement/AccountDeleterequest",
                        content
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Unable to submit delete request."
                        });
                    }

                    var apiResponse = await response.Content.ReadAsStringAsync();
                    var responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse);

                    return Json(responseModel);
                }
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Something went wrong. Please try again."
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ClearNotification()
        {
            ResponseModel responseModel = new ResponseModel();

            string emailAddress = HttpContext.Session.GetString("UserName")??string.Empty;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/ClearAllNotification/" + emailAddress))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                    }
                }
            }
            return Json(Ok());
        }

        [HttpGet]
        public IActionResult ReloadNotification()
        {
            return ViewComponent("Notification");
        }
        //To invoke delete request by the customer --jayanta(10-12-2025)
        [HttpGet]
        public async Task<IActionResult> InvokeDeleteRequest()
        {
            ResponseModel responseModel = new ResponseModel();

            int csid = HttpContext.Session.GetInt32("UserId")??0;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/InvokeDeleteRequest/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                    }
                    if (responseModel.IsSuccess)
                    {
                        TempData["ModalType"] = "success";
                        TempData["ModalTitle"] = "Success!";
                        TempData["ModalMessage"] = "Your account deletion request has been invoked successfully.";
                    }
                    else
                    {
                        TempData["ModalType"] = "danger";
                        TempData["ModalTitle"] = "Failed!";
                        TempData["ModalMessage"] = "You cannot invoke the account deletion right now, as your request is already processed.";
                    }
                }
            }
            return RedirectToAction("PortalProfile");
        }



        #region




        //for AMS service payment integration
        private InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();
        public async Task<IActionResult> PaymentRequestHandleFromAMS(string orderNumber, string invoiceNumber, int csid, string orderType)
        {
            commonService = new CommonServices();
            string recordType = orderType;

            if (String.IsNullOrEmpty(invoiceNumber))
            {
                //get invoice number by using the order Id



                invoice = await GenerateInvoiceByOrderId(orderNumber, orderType);
            }

            else
            {
                invoice = await GenerateInvoiceFromInvoiceId(invoiceNumber, csid, recordType);
            }

            decimal subtotal = 0, totalTax = 0, totalDiscount = 0, totalAmount = 0;

            if (invoice.invoiceItems != null)
            {
                foreach (var item in invoice.invoiceItems)
                {
                    decimal itemTax = (item.Igstamount ?? 0) + (item.Cgstamount ?? 0) + (item.Sgstamount ?? 0) + (item.Utgstamount ?? 0);
                    decimal discountAmount = item.DiscountPercentage.HasValue ? item.ServiceAmount * item.DiscountPercentage.Value / 100 : 0;

                    subtotal += item.ServiceAmount;
                    totalTax += itemTax;
                    totalDiscount += discountAmount;

                }
            }
            totalAmount = subtotal - totalDiscount + totalTax;
            var requestData = new PayPhiPaymentRequest
            {
                merchantId = Common._merchantId,
                merchantTxnNo = TransactionIdGenerator.GenerateRandomTxnNo(),
                amount = Math.Round(totalAmount, 2) + "",
                txnDate = DateTime.Now.ToString("yyyyMMddHHmmss"),
                customerEmailID = invoice.customerDetails?.EmailAddress ?? String.Empty,
                customerMobileNo = invoice.customerDetails?.Phonenumber ?? String.Empty,
                //returnURL = "https://qa.phicommerce.com/pg/api/v2/authRedirect",
                returnURL = Common.PaymentReturnUrlForAddonService(),
                addlParam1 = "01234",
                addlParam2 = "56789",
                currencyCode = "356",
                payType = "1", // <-- FIX APPLIED HERE
                transactionType = "SALE",
                PayPhiGatewayUrl = "https://qa.phicommerce.com/pg/api/v2/initiateSale"
                // currencyCode, payType, transactionType, and PayPhiGatewayUrl remain fixed defaults
            };

            // ----------------------------------------------------------------------
            // 2. Calculate Secure Hash
            // ----------------------------------------------------------------------
            requestData.secureHash = Common.CalculatePayPhiSecureHash(requestData);

            // ----------------------------------------------------------------------
            // 3. Execute Server-to-Server POST Request
            // ----------------------------------------------------------------------
            var formParams = new Dictionary<string, string>
        {
            // Use the STATIC values from the requestData object
            { "merchantId", requestData.merchantId },//done
            { "merchantTxnNo", requestData.merchantTxnNo },//done
            { "amount", requestData.amount },//done
            { "currencyCode", requestData.currencyCode },//done
            { "payType", requestData.payType },//done
            { "transactionType", requestData.transactionType },//done
            { "txnDate", requestData.txnDate },//done
            { "customerEmailID", requestData.customerEmailID },//done
            { "customerMobileNo", requestData.customerMobileNo },//done
            { "returnURL", requestData.returnURL },//done
            { "addlParam1", requestData.addlParam1 },//done
            { "addlParam2", requestData.addlParam2 },//done
            { "paymentMode", requestData.paymentMode },//done
            { "secureHash", requestData.secureHash }//done
        };

            using var httpClient1 = new HttpClient();
            //var content = new FormUrlEncodedContent(formParams);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(formParams), Encoding.UTF8, "application/json");

            try
            {
                //HttpResponseMessage response = await httpClient.PostAsync(requestData.PayPhiGatewayUrl, content);
                HttpResponseMessage response = await httpClient1.PostAsync(requestData.PayPhiGatewayUrl, stringContent);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = System.Text.Json.Nodes.JsonNode.Parse(responseBody)?.AsObject();

                    //un-comment it when realtime payment API integration will be done-Anshuman 24.10.2025

                    if (!string.IsNullOrEmpty(jsonResponse?["redirectURI"]?.ToString() ?? String.Empty))
                    {
                        return Redirect(jsonResponse?["redirectURI"]?.ToString() + "?tranCtx=" + jsonResponse?["tranCtx"]);
                    }
                    else
                    {
                        //return Redirect(requestData.returnURL);
                        return View(invoice);
                    }
                }
                else
                {
                    //return Redirect(requestData.returnURL);
                    return View(invoice);
                }
            }
            catch (Exception ex)
            {
                //return Redirect(requestData.returnURL);
                return View(invoice);
            }

        }
        public async Task<IActionResult> HandleAddOnServicePaymentResponseForAMS(PaymentResponse paymentResponse)
        {
            ResponseModel responseModel = new ResponseModel();
            ViewBag.TransactionId = paymentResponse.TxnID;
            ViewBag.OrderType = "Invoice";
            string paymentStatus = (paymentResponse.ResponseCode == "R1000" || paymentResponse.ResponseCode == "000" || paymentResponse.ResponseCode == "0000") ? "Success" : "Failed";
            string orderId = invoice.OrderNumber ?? String.Empty;
            ViewBag.paymentStatus = paymentStatus;//string paymentStatus = "Success";
            //orderDetails.paymentResponse = paymentResponse;


            //yet to be done
            if (paymentStatus == "Success")
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "OrderMaster/CustomerOrderUpdateFromAMS", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                        }
                    }
                }

            }


            //if (!String.IsNullOrEmpty(invoice.OrderNumber))
            //{
            //    InvoicePrintDetailsViewModel invoiceDetails = new InvoicePrintDetailsViewModel();

            //    // Define the file path (relative to wwwroot)
            //    const string LogoPathAndName = "assets/images/Cloudtree_final_Logo.png";

            //    // 1. Pass the server's root path and the image name/path to the helper
            //    string base64Logo = ImageToString.GetBase64Logo(_env.WebRootPath, LogoPathAndName);

            //    // 2. Attach the finished string to the ViewModel
            //    // (Ensure your GenerateInvoiceViewModel has a Base64LogoString property)
            //    invoice.Base64LogoString = base64Logo;
            //    string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);

            //    // 2. CONVERT HTML STRING TO PDF BYTE ARRAY
            //    byte[] pdfBytes = GeneratePdfFromHtml(invoiceHtml);

            //    // 3. SEND EMAIL WITH PDF ATTACHMENT
            //    string toEmail = orderDetails.EmailAddress ?? String.Empty;
            //    string contactPerson = orderDetails.ContactPersonName ?? String.Empty;

            //    //sendEmail(toEmail, password, toEmail, contactPerson, pdfBytes, $"Invoice_{OrderDetailViewModel.InvoiceId}.pdf");
            //    sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{orderDetails.InvoiceId}.pdf");
            //}
            //else
            //{
            //    return RedirectToAction("Login", "CustomerManagement");
            //    //return View("PaymentResponse", OrderDetailViewModel);
            //}


            InvoicePrintDetailsViewModel dpinvoiceDetails = new InvoicePrintDetailsViewModel();
            dpinvoiceDetails = invoice;
            invoice = new InvoicePrintDetailsViewModel();
            return View(dpinvoiceDetails);
        }

        private async Task<InvoicePrintDetailsViewModel> GenerateInvoiceByOrderId(string orderId, string orderType)
        {
            InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + orderId + "/" + orderType))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse) ?? new InvoicePrintDetailsViewModel();
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            return invoice;
        }
        private async Task<InvoicePrintDetailsViewModel> GenerateInvoiceFromInvoiceId(string invoiceNumber, int csid, string recordtype)
        {
            InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();
            //for sending the payment link to the customer email
            using (var httpClient1 = Common.GetHttpClient())
            {
                using (var response1 = await httpClient1.GetAsync(Common.GetBaseAddress() + "AmsCustomerMaster/GenerateInvoiceFromInvoiceId/" + invoiceNumber + "/" + csid + "/" + recordtype))
                {
                    if (response1.IsSuccessStatusCode)
                    {
                        string apiResponse1 = await response1.Content.ReadAsStringAsync();
                        invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse1) ?? new InvoicePrintDetailsViewModel();
                    }
                    else
                    {
                        string error = await response1.Content.ReadAsStringAsync();
                    }
                }
            }
            return invoice;
        }
        #endregion


        [HttpGet]
        public async Task<IActionResult> GetResourceUsageData(int companyId) // Changed name to reflect returning data
        {
            PortalDashboardViewModel portalDashboardView = new PortalDashboardViewModel();
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetEmployeeCountByGroupCompanyId/" + csid+"/" + companyId))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        portalDashboardView = JsonConvert.DeserializeObject<PortalDashboardViewModel>(apiResponse) ?? new PortalDashboardViewModel();

                        var resourceData = new
                        {
                            employeeUsed = portalDashboardView.employeeRegisteredCount,
                            employeeTotal = portalDashboardView.totalAllotedemployeeRegisteredCount,
                            labels = new[] { "Registered Employees", "Remaining Capacity" }
                        };

                        return Json(resourceData); // Return data as JSON
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, new { message = "Error fetching dashboard data.", details = error });
                    }
                }
            }
        }






    }
}
