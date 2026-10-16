using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using HCMPortal.Helper;
using HCMPortal.Interface;
using HCMPortal.Models;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.Elfie.Model.Structures;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NReco.PdfGenerator;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Plugins;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using QuestPDF;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using static System.Net.WebRequestMethods;
using static System.Reflection.Metadata.BlobBuilder;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;


namespace HCMPortal.Controllers
{
    public class OrderController : Controller
    {
        ILogger<OrderController> logger;
        private readonly IConfiguration _config;
        private CommonServices? commonService;
        private readonly IWebHostEnvironment _env;
        private PaymentResponse paymentResponse = new PaymentResponse();
        public OrderController(ILogger<OrderController> _logger, IWebHostEnvironment env, IConfiguration config)
        {
            logger = _logger;
            _env = env;
            _config = config;
        }
        //public async Task<IActionResult> CheckoutDetails(int sid, int pid)
        //for track the upgrade from a trial inorder to delete the trial--Anshuman 07.02.2026
        public async Task<IActionResult> CheckoutDetails(int sid, int pid, bool isupgradeFromTrial = false)
        {
            commonService = new CommonServices();
            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel();
            ResponseModel responsemodel = new ResponseModel();
            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (HttpContext?.Session.GetString("UserName") == null)
            {
                TempData["ReturnUrl"] = Url.Action("CheckoutDetails", "Order", new { sid = sid, pid = pid });
                //TempData["ReturnUrl"] = Url.Action("CheckoutDetails", "Order");
                // return RedirectToAction("Login", "CustomerManagement");
                return RedirectToAction("RegisterHCMPortalCustomer", "CustomerManagement");
            }
            //to stop the multiple susbcription -- Anshuman 07.02.2026
            if ((await commonService.StopMultipleSubscription(csid)).IsExist)
            {
                TempData["SubscriptionWarning"] = "You already have a subscription on your account. Please visit your My Subscription section to manage or renew your plan.";
                return RedirectToAction("SubscriptionDetails", "Subscription");
            }






            if (TempData.ContainsKey("ReturnUrl"))
            {
                TempData.Remove("ReturnUrl");
            }


            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");
            ViewBag.StateList = commonService.StateList();
            //take the generator method to the commonservice class as it is a helper--Anshuman 29.10.2025
            orderDetailViewModel.OrderId = commonService.GenerateOrderNumber();
            TblTaxMaster taxMaster = new TblTaxMaster();
            taxMaster = await commonService.getTaxdetails();
            List<PlanMaster>? PlanMaster = await commonService.getAllPlans();
            orderDetailViewModel.ListOfPlanMater = PlanMaster;


            orderDetailViewModel.planMaster = PlanMaster.Where(e => e.PlanId == pid).FirstOrDefault();
            orderDetailViewModel.TaxPercentage = (float)(taxMaster != null ? taxMaster.TaxPercentage : 0);
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();



            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/GetHCMportalcustomerdetails/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        portalCredentialViewModel = JsonConvert.DeserializeObject<PortalCredentialViewModel>(apiResponse) ?? new PortalCredentialViewModel();

                    }
                }
            }
            orderDetailViewModel.PortalCustomerDetails = portalCredentialViewModel;


            List<SubscriptionDetailsViewModel> subscriptionDetails = new List<SubscriptionDetailsViewModel>();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetsuscriptionDetailsWithBillingTypeId"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        subscriptionDetails = JsonConvert.DeserializeObject<List<SubscriptionDetailsViewModel>>(apiResponse) ?? new List<SubscriptionDetailsViewModel>();
                    }
                }
            }
            //if(orderDetailViewModel.planMaster.PlanId==0)
            //{
            //    pid= orderDetailViewModel.planId;
            //}
            orderDetailViewModel.ListOfPlanMater = orderDetailViewModel.ListOfPlanMater.Where(e => subscriptionDetails.Any(x => x.Bid == e.PlanId && x.Amount > 0)).ToList();
            SubscriptionDetailsViewModel fortrialTracking = subscriptionDetails.Where(x => x.Bid == pid && x.Sid == sid).ToList().FirstOrDefault() ?? new SubscriptionDetailsViewModel();
            subscriptionDetails = subscriptionDetails.Where(x => x.Bid == pid && x.Amount > 0).ToList();
            //subscriptionDetails = subscriptionDetails.Where(x => x.Bid == pid).ToList();
            if (sid == 0)
            {
                sid = subscriptionDetails.Where(x => x.IsDelete == 0).Select(x => x.Sid).FirstOrDefault();
                // orderDetailViewModel.SelectedSubscription = subscriptionDetails.FirstOrDefault();
            }
            orderDetailViewModel.SelectedSubscription = subscriptionDetails.Where(x => x.Sid == sid).FirstOrDefault();
            orderDetailViewModel.SubscriptionDetails = subscriptionDetails.Where(x => x.Sid != sid).ToList();

            ////to////
            //if (subscriptionDetails.Any(e => orderDetailViewModel.ListOfPlanMater.Any(x => x.PlanId == e.Bid) && e.Amount == 0))
            //{
            //    List<int> planId = subscriptionDetails.GroupBy(x => x.Bid).Where(g => g.All(p => p.Amount> 0)).Select(g => g.Key).ToList();
            //    foreach (var item in planId)
            //    {
            //        if (subscriptionDetails.DistinctBy(e => new { e.Sid, item }).ToList().Any(e => e.Amount == 0))
            //        {
            //            PlanMaster plan = orderDetailViewModel.ListOfPlanMater.FirstOrDefault(e => e.PlanId == item) ?? new PlanMaster();
            //            orderDetailViewModel.ListOfPlanMater.Remove(plan);
            //        }
            //    }
            //}
            ////to////

            //here needs to implement the trial account initiate as on trial we can not let the customre into order page
            //if (orderDetailViewModel.SelectedSubscription?.Sbfid != 0
            //    && orderDetailViewModel.SelectedSubscription?.ValidUptoDays != null
            //    && orderDetailViewModel.SelectedSubscription?.ValidUptoDays >= 0
            //    && orderDetailViewModel.SelectedSubscription?.Amount == 0)
            if (fortrialTracking.Sbfid != 0 && fortrialTracking.ValidUptoDays != null && fortrialTracking.ValidUptoDays >= 0 && fortrialTracking.Amount == 0)
            {
                ResponseModel responseModel1 = new ResponseModel();
                TblCustomerOrderDetail customerOrder = new TblCustomerOrderDetail()
                {

                    Csid = portalCredentialViewModel.Csid,
                    //InvoiceId = (orderDetail.paymentResponse?.ResponseCode == "000" || orderDetail.paymentResponse?.ResponseCode == "0000") ? UniqueIdGenerator.GetUniqueInvoiceNumber() : "",
                    OrderId = GenerateOrderNumber(),
                    SubscriptionBillingFrequencyId = fortrialTracking.Sbfid,
                    SubscriptionAmount = 0,
                    DiscountId = 0,//later implemented as not yet done
                    TaxAmount = 0,
                    NetAmount = 0,
                    BillingAddress = null,
                    OrderStatus = "Completed",
                    CreatedDate = DateTime.Now,
                    CreatedBy = portalCredentialViewModel.ContactPersonName + "@" + portalCredentialViewModel.Csid,
                    TransactionId = null,
                    TransactionDate = DateTime.Now,
                    TransactionStatus = null,
                    PurchasedDate = DateTime.Now,
                    OrderType = "Subscription", //later implemented as not yet done
                    SubscriptionStartDate = DateTime.Now,
                    //MembershipId = customerAlphanumericId
                };
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(customerOrder), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "OrderMaster/SaveOrderDetailsOnTrial", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            responseModel1 = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                            if (responseModel1.IsExist)
                            {
                                TempData["SubscriptionWarning"] = "Trial Period Exhausted! Your free trial period has ended or its usage limits have been reached.\r\n                                                To continue using the features, please Subscribe Now to one of the paid plans below.";
                                return RedirectToAction("SubscriptionDetails", "Subscription");
                            }
                            string contactPerson = portalCredentialViewModel.ContactPersonName;
                            string userName = portalCredentialViewModel.EmailAddress ?? string.Empty;
                            string userUrl = Common.GetLandingPageAddress(); // Replace with your actual sign-in URL if different from base
                            string userKey = APIEncryptionHelper.Encrypt(userName); // Assuming ResponseModel has a UserKey property

                            // --- 1. Define Trial Duration ---
                            const int trialDurationDays = 15;

                            // --- 2. Calculate Trial Expiration Date ---
                            DateTime trialStartDate = DateTime.Now; // Or DateTime.UtcNow, depending on your timezone preference
                            DateTime trialExpirationDate = trialStartDate.AddDays(trialDurationDays);

                            // Format the date nicely for the email (e.g., "December 25, 2025")
                            string formattedExpirationDate = trialExpirationDate.ToString("MMMM dd, yyyy");

                            // --- 3. Prepare the HTML Email Body String ---
                            // Using the enhanced, mobile-friendly HTML structure from the previous response

                            string body = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome Email</title>
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
            background: #007bff; /* Blue for a welcome/primary action */
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
        .email-body a {{
            color: #007bff;
            text-decoration: none;
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
            <h1>Welcome to HCM!</h1>
        </div>
        <div class=""email-body"">
            <p>Dear {contactPerson}</p>
            <p>Thank you for registering with <strong>HCM</strong>! We’re thrilled to have you as part of our community.</p>
            
            <p>🎉 Your **{trialDurationDays} day trial account** is now **active**! You can start exploring all the features of the HCM platform today.</p>
            <p>Your trial is scheduled to expire on **{formattedExpirationDate}**.</p>
            
            <p><strong>Here are your account details:</strong></p>
            <ul>
                <li><strong>Username:</strong> {userName}</li>
            
            </ul>
            
            <table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"" style=""margin: 20px 0;"">
                <tr>
                    <td align=""center"">
                        <table cellpadding=""0"" cellspacing=""0"" border=""0"">
                            <tr>
                                <td align=""center"" style=""border-radius: 4px; background-color: #556ee6; padding: 10px 25px;"">
                                    <a href=""{userUrl}?userKey={userKey}"" target=""_blank"" style=""font-size: 16px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; font-weight: bold; display: inline-block;"">
                                        Sign In
                                    </a>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            <p><strong>Next Steps:</strong></p>
            <ol>
                <li>Log in to your account using the credentials above.</li>
                <li>Start exploring the HCM platform to get the most out of your account.</li>
            </ol>
            
            <p>If you have any questions or need help, our support team is just a message away at <a href=""mailto:info@thecloudtree.ai"">info@thecloudtree.ai</a>.</p>
            
            <p>Welcome aboard!</p>
            <p>Your invoice has been attached to this email.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM</p>
            <p>info@thecloudtree.ai Ph : (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>
";
                            // --- 4. Sending the Email ---
                            string toEmail = portalCredentialViewModel.EmailAddress ?? string.Empty;

                            // This is the line that executes the email send operation
                            EmailHelper.sendEmail(toEmail, body, "Welcome to HCM!", null, string.Empty);
                            TempData["TrialStart"] = "Your Trial Subscription Has Started Successfully";
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                        }
                    }

                }
                return RedirectToAction("PortalDashboard", "CustomerManagement");
            }


            TblMyCart tblMyCart = new TblMyCart();
            tblMyCart.UserId = portalCredentialViewModel.Csid;
            tblMyCart.OrderNo = orderDetailViewModel.OrderId;
            tblMyCart.Sid = sid;
            tblMyCart.Pid = pid;
            tblMyCart.IsDelete = 0;
            tblMyCart.CreatedDate = DateTime.Now;
            tblMyCart.CreatedBy = portalCredentialViewModel.Csid;
            tblMyCart.TransactionStatus = "Pending";
            tblMyCart.OrderStatus = "Pending";
            ResponseModel responseModel = new ResponseModel();
            using (var httpClient = Common.GetHttpClient())
            {

                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(tblMyCart), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "OrderMaster/SaveMyCart", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                    }
                }
            }
            responsemodel = await commonService.CheckCustomerSubscriptionExists(HttpContext.Session.GetInt32("UserId") ?? 0);
            if (responsemodel.IsExist == true)
            {

                orderDetailViewModel.issubscriptionActive = true;

            }
            orderDetailViewModel.planId = pid;
            orderDetailViewModel.isupgradeFromTrial = isupgradeFromTrial;

            return View(orderDetailViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> CheckEmailDuplicacy(string primaryEmail)
        {
            ResponseModel model = new ResponseModel();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsPEmailExist/" + primaryEmail))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            model = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }

                }
                return Json(!model.IsExist);

            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckPhoneDuplicacy(string primaryPhone)
        {
            ResponseModel model = new ResponseModel();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsPMobileNoExist/" + primaryPhone))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            model = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }

                }
                return Json(!model.IsExist);

            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckOrganisationDuplicacy(string organisation)
        {
            ResponseModel model = new ResponseModel();
            try
            {

                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsCompNameExist/" + organisation))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            model = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }

                }

                return Json(!model.IsExist);

            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckGSTINDuplicacy(string modalGSTIN)
        {
            ResponseModel model = new ResponseModel();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsGSTINExist/" + modalGSTIN))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            model = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }

                }
                return Json(!model.IsExist);

            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckPANDuplicacy(string modalPAN)
        {
            ResponseModel model = new ResponseModel();
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsPanExist/" + modalPAN))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            model = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                        }
                    }

                }
                return Json(!model.IsExist);

            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public async Task<IActionResult> ClearMycart()
        {
            ResponseModel responseModel = new ResponseModel();

            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/clearMyCart/" + csid))
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
        public IActionResult ReloadCart()
        {
            return ViewComponent("MyCart");
        }

        [HttpGet]
        public async Task<IActionResult> GetSubscriptionsByPlanId(int planId, int sid)
        {
            List<SubscriptionDetailsViewModel> subscriptions = new List<SubscriptionDetailsViewModel>();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetsuscriptionDetailsWithBillingTypeId"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        subscriptions = JsonConvert.DeserializeObject<List<SubscriptionDetailsViewModel>>(apiResponse) ?? new List<SubscriptionDetailsViewModel>();
                    }
                    //subscriptions = subscriptions.Where(e => e.Bid == planId && e.Amount > 0 && e.Sid !=sid).ToList();
                    subscriptions = subscriptions.Where(e => e.Bid == planId && e.Amount > 0).ToList();

                }
            }
            return Json(subscriptions);
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

        [HttpGet("customer/mybillings", Name = "BillingDetails")]
        public async Task<IActionResult> BillingDetails()
        {
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            List<CustomerOrderDetails> orderDetails = new List<CustomerOrderDetails>();
            using (var httpClient = Common.GetHttpClient())
            {

                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSCustomerLedger/GetCustomerOrderDetailsForHCMPortal/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        orderDetails = JsonConvert.DeserializeObject<List<CustomerOrderDetails>>(apiResponse) ?? new List<CustomerOrderDetails>();
                        orderDetails = orderDetails.Where(e => e.OrderStatus != "Draft").ToList();
                    }
                }
            }

            return View(orderDetails);
        }
        [HttpGet]
        public async Task<IActionResult> InvoicePrint(int Id, string RecordType)
        {
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSCustomerLedger/InvoicePrintDetailsForHCMPortal/" + Id + "/" + RecordType + "/" + csid))
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
            if (invoice.SelfDetails == null || invoice.SelfDetails.CompanyId == 0)
            {
                return BadRequest("Missing Self company Details.Please Add your Organization Information and Then try.");
            }
            if (invoice.customerDetails == null || invoice.customerDetails.Csid == 0)
            {
                return BadRequest("Missing Customer Details.Please Add At least One and Then try.");
            }
            string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = invoice.OrderNumber, orderType = invoice.RecordType }, Request.Scheme) ?? String.Empty;
            invoice.QrCodeBase64Image = GenerateQrCodeBase64.GenerateQrCode(urlForQr);
            return PartialView("_InvoicePrintPartialView", invoice);
        }





        [HttpGet("order/book-a-demo", Name = "DemoBook")]
        //[HttpGet("hcm-solution/order/book-a-demo", Name = "ProductDemoBook")]
        [HttpGet("hcm-solution/book-a-demo", Name = "ProductDemoBook", Order = 0)]
        // for URL optimize--Anshuman 13.07.2026
        public async Task<IActionResult> DemoBookingDetails()
        {
            DemoBookingViewModel demoBookingViewModel = new DemoBookingViewModel();
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();
            commonService = new CommonServices();
            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");
            demoBookingViewModel.CountryId = commonService.CountryList().Where(e => e.CountryName == "India").Select(e => e.CountryId).FirstOrDefault();
            ViewBag.StateList = commonService.StateList();

            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
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
                    }
                }
                demoBookingViewModel.Name = portalCredentialViewModel.ContactPersonName;
                demoBookingViewModel.Email = portalCredentialViewModel.EmailAddress;
                demoBookingViewModel.Address = portalCredentialViewModel.Address;
                demoBookingViewModel.Phone = portalCredentialViewModel.Phonenumber;
                demoBookingViewModel.StateId = portalCredentialViewModel.StateId ?? 0;
                demoBookingViewModel.IndustryTypeId = Convert.ToInt32(portalCredentialViewModel.IndustryTypeId);

            }

            commonService = new CommonServices();
            ViewBag.Source = commonService.GetAllSource();
            ViewBag.IndusryType = commonService.IndustyList();
            ViewBag.Success = TempData["Success"];
            ViewBag.Exception = TempData["Error"];
            return View(demoBookingViewModel);
        }


        [HttpPost("hcm-solution/book-a-demo")]
        public async Task<IActionResult> DemoBookingDetails(DemoBookingViewModel model, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            ResponseModel responseModel = new ResponseModel();
            commonService = new CommonServices();
            ViewBag.CountryList = commonService.CountryList().Where(e => e.CountryName == "India");
            ViewBag.StateList = commonService.StateList();
            ViewBag.Source = commonService.GetAllSource();
            ViewBag.IndusryType = commonService.IndustyList();
            //if (!await VerifyRecaptcha(gRecaptchaResponse))
            //{
            //    ModelState.AddModelError("", "reCAPTCHA validation failed.");
            //    //TempData["Error"]="Validate All required field";
            //    return View(model);
            //}
            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
            //if (!ModelState.IsValid)
            //    return View(model);
            //if (ModelState.IsValid)
            //{
            using (var httpClient = Common.GetHttpClient())
            {

                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "OrderMaster/SaveDemoRequestDetails", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();

                        if (responseModel.IsSuccess)
                        {
                            TempData["Success"] = responseModel.Message;
                            string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
    "<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
    "<title>Booking Request Received</title>\r\n" +
    "<style>\r\n" +
    "body {\r\n" +
    "    font-family: Arial, sans-serif;\r\n" +
    "    line-height: 1.6;\r\n" +
    "    color: #333;\r\n" +
    "    background-color: #f9f9f9;\r\n" +
    "    margin: 0;\r\n" +
    "    padding: 0;\r\n" +
    "}\r\n" +
    ".email-container {\r\n" +
    "    max-width: 600px;\r\n" +
    "    margin: 20px auto;\r\n" +
    "    background: #ffffff;\r\n" +
    "    border: 1px solid #ddd;\r\n" +
    "    border-radius: 8px;\r\n" +
    "    overflow: hidden;\r\n" +
    "    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\r\n" +
    "}\r\n" +
    ".email-header {\r\n" +
    "    background: #007bff;\r\n" +
    "    color: #ffffff;\r\n" +
    "    padding: 20px;\r\n" +
    "    text-align: center;\r\n" +
    "}\r\n" +
    ".email-header h1 {\r\n" +
    "    margin: 0;\r\n" +
    "    font-size: 24px;\r\n" +
    "}\r\n" +
    ".email-body {\r\n" +
    "    padding: 20px;\r\n" +
    "}\r\n" +
    ".email-body p {\r\n" +
    "    margin: 0 0 15px;\r\n" +
    "}\r\n" +
    ".email-body a {\r\n" +
    "    color: #007bff;\r\n" +
    "    text-decoration: none;\r\n" +
    "}\r\n" +
    ".email-footer {\r\n" +
    "    background: #f4f4f4;\r\n" +
    "    text-align: center;\r\n" +
    "    padding: 15px;\r\n" +
    "    font-size: 14px;\r\n" +
    "    color: #777;\r\n" +
    "}\r\n" +
    "</style>\r\n" +
    "</head>\r\n" +
    "<body>\r\n" +
    "<div class=\"email-container\">\r\n" +
    "    <div class=\"email-header\">\r\n" +
    "        <h1>Booking Request Received ✅</h1>\r\n" +
    "    </div>\r\n" +
    "    <div class=\"email-body\">\r\n" +
    "        <p>Dear " + model.Name + ",</p>\r\n" +
    "        <p>Thank you for reaching out to <strong>TheCloudTree</strong>. We have successfully received your booking request.</p>\r\n" +
    "        <p>Our team is currently reviewing your request, and one of our executives will get in touch with you shortly to confirm the details and assist you with the next steps.</p>\r\n" +
    "        <p>Meanwhile, if you have any urgent questions or additional information to share, please feel free to contact us.</p>\r\n" +
    //"        <p><strong>Contact Details:</strong></p>\r\n" +
    //"        <ul>\r\n" +
    //"            <li>Email: <a href=\"mailto:info@thecloudtree.ai\">info@thecloudtree.ai</a></li>\r\n" +
    //"            <li>Phone: (+91) 9090973033</li>\r\n" +
    //"        </ul>\r\n" +
    "        <p>We look forward to serving you!</p>\r\n" +
    //"        <p>Warm regards,<br><strong>The TheCloudTree Team</strong></p>\r\n" +
    "    </div>\r\n" +
    "    <div class=\"email-footer\">\r\n" +
    "        <p>Best regards,<br>thecloudTree.ai<br>The CloudTree</p>\r\n" +
    "        <p>info@thecloudtree.ai &nbsp;|&nbsp; Ph: (+91) 9090973033</p>\r\n" +
    "    </div>\r\n" +
    "</div>\r\n" +
    "</body>\r\n" +
    "</html>";

                            if (model.Email != null)
                            {
                                SendEmail(model.Email, "Thank You! We’ll Contact You Soon to Schedule Your Demo", body);
                            }

                        }
                        else
                        {
                            TempData["Error"] = "Something went wrong! Please try again";
                        }
                    }
                }
            }
            //}
            //else
            //{
            //    TempData["Error"] = "Validate All required field";
            //    return View(model);
            //}



            return View(model);

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
        public async Task<IActionResult> GetDiscountDetailsByCode(int pid, int sid, string companyType, string dmcode)
        {

            int companytypeid = 0;
            if (companyType == "Group")
            {
                companytypeid = 2;
            }
            else
            {
                companytypeid = 1;
            }
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            DiscountMasterViewModel discountMasterViewModel = new DiscountMasterViewModel();
            if (dmcode != null)
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Discount/GetDiscountDetailsByCode/" + dmcode))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            discountMasterViewModel = JsonConvert.DeserializeObject<DiscountMasterViewModel>(apiResponse) ?? new DiscountMasterViewModel();

                        }
                    }
                }
            }


            //    bool isApplicable =
            //(discountMasterViewModel.subscriptionIds.Contains(sid) || !discountMasterViewModel.subscriptionIds.Any()) &&
            //(discountMasterViewModel.planIds.Contains(pid) || !discountMasterViewModel.planIds.Any()) &&
            //(discountMasterViewModel.ApplicableCompanyTypeIds.Contains(companytypeid) || !discountMasterViewModel.ApplicableCompanyTypeIds.Any()) &&
            //(discountMasterViewModel.ApplicableCustomerIds.Contains(UserId) || !discountMasterViewModel.ApplicableCustomerIds.Any());

            //    if (!isApplicable)
            //    {
            //        // ❌ Discount code not applicable for this combination
            //        return Json(new { discountType = "None", discountValue = 0 });
            //    }
            commonService = new CommonServices();

            int usagecount = 0;
            usagecount = await commonService.getdiscountUsagecount(discountMasterViewModel.Dmid, UserId);
            if (discountMasterViewModel.LifecycleType?.ToLower() == "onetime" && usagecount >= 1)
            {
                return Json(new { discountType = "Used", discountValue = 0, message = "You have already used this discount once." });
            }

            if (discountMasterViewModel.LifecycleType?.ToLower() == "repeat" && usagecount >= 3)
            {
                return Json(new { discountType = "Used", discountValue = 0, message = "You have reached the maximum usage limit for this discount." });
            }
            //if (discountMasterViewModel.Dmid > 0)
            //    {
            //        return Json(new { discountType = "None", discountValue = 0 });
            //    }
            return Json(new
            {
                DMId = discountMasterViewModel.Dmid,
                discountType = discountMasterViewModel.DiscountType,
                discountValue = discountMasterViewModel.DiscountValue
            });


        }


        //[HttpGet]
        //public async Task<IActionResult> GetSlotsByDay(string dayName, string inputDate)
        //{
        //    List<TblSlotMaster> slots = new List<TblSlotMaster>();

        //    using (var httpClient = Common.GetHttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "ManageSlot/GetSlotlist/" + dayName))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string apiResponse = await response.Content.ReadAsStringAsync();
        //                slots = JsonConvert.DeserializeObject<List<TblSlotMaster>>(apiResponse) ?? new List<TblSlotMaster>();

        //            }
        //        }
        //    }
        //    DateTime today = DateTime.Today;
        //    TimeOnly nowTime = TimeOnly.FromDateTime(DateTime.Now);
        //    if (Convert.ToDateTime(inputDate).Date == today)
        //    {
        //        slots = slots
        //            .Where(s =>
        //                s.SlotStartTime.HasValue &&
        //                s.SlotStartTime.Value > nowTime
        //            )
        //            .ToList();
        //    }
        //    //var slots = context.TblSlotMasters
        //    //                   .Where(x => x.Day == day && x.IsDelete == 0)
        //    //                   .Select(x => new {
        //    //                       slotId = x.SlotId,
        //    //                       slotName = x.SlotName,
        //    //                       day = x.Day,
        //    //                       slotStartTime = x.SlotStartTime.ToString(),
        //    //                       slotEndTime = x.SlotEndTime.ToString()
        //    //                   })
        //    //                   .ToList();

        //    return Json(slots);
        //}
        [HttpGet]
        //public async Task<IActionResult> GetSlotsByDay(string dayName, string inputDate)
        //changes by Anshuman 24.02.2026
        public async Task<IActionResult> GetSlotsByDay(string dayName, DateTime inputDate)
        {
            List<TblSlotMaster> slots = new List<TblSlotMaster>();

            // Normalize day name (important for server)
            dayName = dayName?.Trim();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(
                    Common.GetBaseAddress() + "ManageSlot/GetSlotlist/" + dayName))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        slots = JsonConvert.DeserializeObject<List<TblSlotMaster>>(apiResponse)
                                ?? new List<TblSlotMaster>();
                    }
                }
            }

            //// 🔴 SERVER-SAFE DATE PARSING (CRITICAL FIX)
            //if (!DateTime.TryParseExact(
            //        inputDate,
            //        "dd-MM-yyyy",
            //        CultureInfo.InvariantCulture,
            //        DateTimeStyles.None,
            //        out DateTime selectedDate))
            //{
            //    return Json(new List<TblSlotMaster>());
            //}

            DateTime today = DateTime.Today;
            TimeOnly nowTime = TimeOnly.FromDateTime(DateTime.Now);

            // Filter only future slots if today
            //if (selectedDate.Date == today)
            if (inputDate.Date == today)
            {
                slots = slots
                    .Where(s =>
                        s.SlotStartTime.HasValue &&
                        s.SlotStartTime.Value > nowTime
                    )
                    .ToList();
            }

            return Json(slots);
        }


        public IActionResult BookingConfirmation()
        {
            ViewBag.EventLink = TempData["EventLink"];
            return View();
        }

        // The Merchant ID for testing
        private readonly string _merchantId = "T_03342";

        // !!! CRITICAL: Replace "abc" with the actual Merchant Secret Key for T_03342 !!!
        private const string MerchantSecretKey = "abc";
        private static OrderDetailViewModel OrderDetailViewModel = new OrderDetailViewModel();


        public string CalculatePayPhiSecureHash(PayPhiPaymentRequest request)
        {
            var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "addlParam1", request.addlParam1 },
            { "addlParam2", request.addlParam2 },
            { "amount", request.amount },
            { "currencyCode", request.currencyCode },
            { "customerEmailID", request.customerEmailID },
            { "customerMobileNo", request.customerMobileNo },
            { "merchantId", request.merchantId },
            { "merchantTxnNo", request.merchantTxnNo },
            { "payType", request.payType },
            { "returnURL", request.returnURL },
             { "paymentMode", request.paymentMode },
            { "transactionType", request.transactionType },
            { "txnDate", request.txnDate },
        };

            var dataToHashBuilder = new System.Text.StringBuilder();
            foreach (var pair in parameters)
            {
                dataToHashBuilder.Append(pair.Value);
            }
            string dataToHash = dataToHashBuilder.ToString();

            var keyBytes = System.Text.Encoding.UTF8.GetBytes(MerchantSecretKey);
            var messageBytes = System.Text.Encoding.ASCII.GetBytes(dataToHash);

            byte[] hashBytes;
            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                hashBytes = hmacsha256.ComputeHash(messageBytes);
            }

            var hashBuilder = new System.Text.StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }
        public async Task<IActionResult> PaymentResponse(PaymentResponse paymentResponse)
        {
            //comment it when realtime payment PAI integration will be done-Anshuman 24.10.2025
            //paymentResponse = new PaymentResponse()
            //{
            //    TxnID="126437141724",
            //    ResponseCode = "000",
            //    RespDescription = "Success",
            //};
            ViewBag.TransactionId = paymentResponse.TxnID;
            ViewBag.OrderType = "Subscription";
            string paymentStatus = (paymentResponse.ResponseCode == "R1000" || paymentResponse.ResponseCode == "000" || paymentResponse.ResponseCode == "0000") ? "Success" : "Failed";
            string orderId = OrderDetailViewModel.OrderId ?? String.Empty;
            ViewBag.paymentStatus = paymentStatus;//string paymentStatus = "Success";
            string password = string.Empty;


            //if (paymentStatus == "Success")
            //{
            OrderDetailViewModel.paymentResponse = paymentResponse;
            using (var httpClient = Common.GetHttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "CustomerCompanySetup/HCMSetupForCustomer", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        password = await response.Content.ReadAsStringAsync();
                        if (!String.IsNullOrEmpty(password))
                        {
                            sendEmail(OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty, password, OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty, OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? String.Empty);
                        }
                        // 1. RENDER THE PARTIAL VIEW TO HTML STRING
                        // NOTE: Make sure OrderDetailViewModel has the same structure as 
                        // the model expected by your partial view (_InvoicePrintPartialView.cshtml)
                        // Or, if OrderDetailViewModel contains the invoice model, pass that sub-model.
                        // Assuming OrderDetailViewModel *is* the model for the invoice view:


                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            //if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
            if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId) && !String.IsNullOrEmpty(password))
            {
                InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/" + "Subscription"))
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
                // Define the file path (relative to wwwroot)
                const string LogoPathAndName = "assets/images/Cloudtree_final_Logo.png";

                // 1. Pass the server's root path and the image name/path to the helper
                string base64Logo = ImageToString.GetBase64Logo(_env.WebRootPath, LogoPathAndName);

                // 2. Attach the finished string to the ViewModel
                // (Ensure your GenerateInvoiceViewModel has a Base64LogoString property)
                invoice.Base64LogoString = base64Logo;
                string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);

                // 2. CONVERT HTML STRING TO PDF BYTE ARRAY
                byte[] pdfBytes = await GeneratePdfFromHtml(invoiceHtml);

                // 3. SEND EMAIL WITH PDF ATTACHMENT
                string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty;
                string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? String.Empty;

                //sendEmail(toEmail, password, toEmail, contactPerson, pdfBytes, $"Invoice_{OrderDetailViewModel.InvoiceId}.pdf");
                sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceId}.pdf");
            }
            else
            {
                return View("PaymentResponse", orderId);
                //return View("PaymentResponse", OrderDetailViewModel);
            }
            //}


            //string OrderStatus = paymentStatus;
            OrderDetailViewModel = new OrderDetailViewModel();
            return View("PaymentResponse", orderId);
        }
        public void sendEmail(string ToEmail, string Password, string UserName, string ContactPerson)
        {
            string userKey = APIEncryptionHelper.Encrypt(ToEmail);
            string url = Common.GetLandingPageAddress();
            string body = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome Email</title>
    <style>

        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
.btn{{
color: #fff;
  background-color: #556ee6;
  border-color: #556ee6;

  display: inline-block;
  font-weight: 400;
  line-height: 1.5;
  color: #495057;
  text-align: center;
  vertical-align: middle;
  cursor: pointer;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
  background-color: transparent;
  border: 1px solid transparent;
  border-top-color: transparent;
  border-right-color: transparent;
  border-bottom-color: transparent;
  border-left-color: transparent;
  padding: .47rem .75rem;
  font-size: .8125rem;
  border-radius: .25rem;
  outline: 0 !important;

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
            background: #007bff; /* Blue for a welcome/primary action */
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
        .email-body a {{
            color: #007bff;
            text-decoration: none;
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
            <h1>Welcome to HCM!</h1>
        </div>
        <div class=""email-body"">
            <p>Dear {ContactPerson}</p>
            <p>Thank you for registering with <strong>HCM</strong>! We’re thrilled to have you as part of our community.</p>
            
            <p><strong>Here are your account details:</strong></p>
            <ul>
                <li><strong>Username:</strong> {UserName}</li>
            
            </ul>
            <p>You can log in to your account anytime at <a class=""btn"" href=""{url}?userKey={userKey}"">Sign In</a>.</p>
            
            <p><strong>Next Steps:</strong></p>
            <ol>
                <li>Log in to your account using the credentials above.</li>
                <li>Start exploring the HCM platform to get the most out of your account.</li>
            </ol>
            
            <p>If you have any questions or need help, our support team is just a message away at <a href=""mailto:info@thecloudtree.ai"">info@thecloudtree.ai</a>.</p>
            
            <p>Welcome aboard!</p>
            <p>Your invoice has been attached to this email.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM</p>
            <p>info@thecloudtree.ai Ph : (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>
";

            // Pass the attachment data and filename to the updated EmailHelper
            EmailHelper.sendEmail(ToEmail, body, "Welcome to HCM!", null, string.Empty);
            //EmailHelper.sendEmail(ToEmail, "Welcome to HCM!", body);



        }

        public void sendEmailWithInvoice(string ToEmail, string ContactPerson, PaymentResponse paymentResponse, byte[] attachmentData, string attachmentFileName)
        {
            string transactionId = paymentResponse.TxnID ?? "N/A";
            string orderId = OrderDetailViewModel.OrderId ?? String.Empty;
            string userKey = APIEncryptionHelper.Encrypt(ToEmail);
            string url = Common.GetLandingPageAddress();
            bool isSuccess = paymentResponse.ResponseCode?.ToLower() == "success";
            string headerColor = isSuccess ? "#28a745" : "#dc3545";
            string headerTitle = isSuccess ? "Purchase Successful!" : "Payment Failed!";
            string mainMessage = isSuccess ? @"Thank you for your purchase from <b>HCM</b>! Your order has been successfully processed."
                : @"Your payment could not be processed. Please try again or contact support.";
            string failedNote = !isSuccess ? "<p style='color:red;'><b>No invoice generated due to failed payment.</b></p>" : "";
            string messagebody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Payment Update</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
        .btn{{
            color: #fff;
            background-color: #556ee6;
            border-color: #556ee6;
            padding: .47rem .75rem;
            font-size: .8125rem;
            border-radius: .25rem;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow: hidden;
        }}
        .email-header {{
            background: {headerColor};
            color: #ffffff;
            padding: 20px;
            text-align: center;
        }}
        .email-body {{
            padding: 20px;
        }}
        .highlight {{
            background-color: #e2f0e6;
            padding: 10px;
            border-radius: 4px;
            border-left: 5px solid {headerColor};
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""email-header"">
            <h1>{headerTitle}</h1>
        </div>
        <div class=""email-body"">
            <p>Dear {ContactPerson},</p>
            <p>{mainMessage}</p>

            <div class=""highlight"">
                <p><strong>Order Details:</strong></p>
                <p>Transaction ID: <b>{transactionId}</b></p>
                <p>Order ID: <b>{orderId}</b></p>
            </div>

            {failedNote}

            {(isSuccess ? @"<p>We've attached your <b>Official Invoice (PDF)</b> to this email.</p>" : "")}

            <p>You can sign in anytime here:
            <a class=""btn"" href=""" + url + @"?userKey=" + userKey + @""">Sign In</a></p>

            <p>If you have any questions, please reply to this email or contact our support team.</p>
        </div>

        <div class=""email-footer"" style=""text-align:center;padding:15px;background:#f4f4f4;font-size:14px;"">
            <p>Best regards,<br>thecloudTree.ai | HCM</p>
            <p>info@thecloudtree.ai | (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>
";

            // send email - invoice only if SUCCESS
            if (isSuccess)
                EmailHelper.sendEmail(ToEmail, messagebody, "Order Purchased Successfully", attachmentData, attachmentFileName);
            else
                EmailHelper.sendEmail(ToEmail, messagebody, "Payment Failed", null, null);
        }

        public async Task<IActionResult> GenerateInvoice(string orderId, string orderType)
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
            if (invoice.SelfDetails == null || invoice.SelfDetails.CompanyId == 0)
            {
                return BadRequest("Missing Self company Details.Please Add your Organization Information and Then try.");
            }
            if (invoice.customerDetails == null || invoice.customerDetails.Csid == 0)
            {
                return BadRequest("Missing Customer Details.Please Add At least One and Then try.");
            }
            string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = orderId, orderType = invoice.RecordType }, Request.Scheme) ?? String.Empty;
            invoice.QrCodeBase64Image = GenerateQrCodeBase64.GenerateQrCode(urlForQr);
            return PartialView("_InvoicePrintPartialView", invoice);
        }

        public async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
        {
            try
            {
                var options = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                    ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
                };
                if (!System.IO.File.Exists(options.ExecutablePath))
                    options.ExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";
                if (!System.IO.File.Exists(options.ExecutablePath))
                    return Array.Empty<byte>();
                using var browser = await Puppeteer.LaunchAsync(options);
                using var page = await browser.NewPageAsync();
                string baseUrl = "https://www.hcmportal.anantatek.com/";
                string htmlWithBase = $"<base href='{baseUrl}' />" + htmlContent;
                await page.EmulateMediaTypeAsync(PuppeteerSharp.Media.MediaType.Print);
                await page.SetContentAsync(htmlWithBase, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });
                var pdfData = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "10mm",
                        Bottom = "10mm",
                        Left = "10mm",
                        Right = "10mm"
                    }
                });
                await browser.CloseAsync();
                return pdfData;
            }
            catch (Exception ex)
            {
                return Array.Empty<byte>();
            }
        }
        private byte[] GeneratePdfFromHtmlold(string htmlContent)
        {
            var htmlToPdf = new HtmlToPdfConverter();

            // Set standard page properties
            htmlToPdf.Orientation = PageOrientation.Portrait;
            htmlToPdf.Size = PageSize.A4;
            htmlToPdf.Zoom = 1.0f;

            // Set margins to ensure nothing is clipped
            htmlToPdf.Margins = new PageMargins { Top = 10, Bottom = 10, Left = 10, Right = 10 };

            // --- CRITICAL IMPROVEMENTS ---

            // 1. Force Print Media Type (You already have this, which is great)
            // This tells the engine to use CSS rules inside @media print { ... }
            htmlToPdf.CustomWkHtmlArgs = "--print-media-type";

            // 2. Add Base URL (Crucial if your HTML uses relative URLs for images/CSS)
            // Replace "https://yourdomain.com/" with the absolute base URL of your application.
            // This allows the PDF generator to correctly find images, fonts, and external CSS.
            // If your logo or styling is failing, this is often the fix!
            // Note: If all your CSS/images are embedded (Base64), you can skip this.
            // htmlToPdf.PageUri = "https://hcm.anantagroup.in.net/"; // Example using your login link domain

            // 3. Enable Backgrounds (If CSS colors or background images aren't showing)
            // Wkhtmltopdf sometimes omits backgrounds for print saving ink.
            if (!htmlToPdf.CustomWkHtmlArgs.Contains("--background"))
            {
                htmlToPdf.CustomWkHtmlArgs += " --no-stop-slow-scripts --enable-javascript --background";
            }

            // -----------------------------

            // Generate PDF byte array
            byte[] pdfBytes = htmlToPdf.GeneratePdf(htmlContent);

            return pdfBytes;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            // Retrieve necessary MVC services from the current HttpContext
            var serviceProvider = HttpContext.RequestServices;
            var razorViewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = serviceProvider.GetRequiredService<ITempDataProvider>();

            // Create an ActionContext for the rendering process
            var actionContext = new ActionContext(HttpContext, RouteData, new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            using (var sw = new StringWriter())
            {
                // 1. Find the View (looks in standard locations like /Views/[Controller]/[ViewName].cshtml)
                var viewResult = razorViewEngine.FindView(actionContext, viewName, isMainPage: false);

                if (viewResult.View == null)
                {
                    // Fallback for shared views
                    viewResult = razorViewEngine.GetView(null, viewName, isMainPage: false);
                    if (viewResult.View == null)
                    {
                        throw new ArgumentNullException($"{viewName} does not match any available view");
                    }
                }

                // 2. Prepare the View Data Dictionary
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model // Sets the @model object in the .cshtml
                };

                // 3. Create the View Context
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                // 4. Render the View
                await viewResult.View.RenderAsync(viewContext);

                // Return the captured HTML string
                return sw.ToString();
            }
        }

        public async Task<IActionResult> HandleRetryPayment(string OrderId)
        {
            if (OrderId != null)
            {
                TblMyCart tblMyCart = new TblMyCart();
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GetCartDetailsByOrderId/" + OrderId))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tblMyCart = JsonConvert.DeserializeObject<TblMyCart>(apiResponse) ?? new TblMyCart();
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                        }
                    }
                }
                return RedirectToAction("CheckoutDetails", new { sid = tblMyCart.Sid, pid = tblMyCart.Pid });
            }
            else
            {
                return RedirectToAction("MySubscriptionDetails", "Subscription");
            }

        }

        private static CustomerOrderViewModel orderDetails = new CustomerOrderViewModel();

        public async Task<IActionResult> HandleAddOnServicePaymentResponse(PaymentResponse paymentResponse)
        {
            //comment it when realtime payment PAI integration will be done-Anshuman 24.10.2025
            //paymentResponse = new PaymentResponse()
            //{
            //    TxnID = "126437141724",
            //    ResponseCode = "000",
            //    RespDescription = "Success",
            //};
            ResponseModel responseModel = new ResponseModel();
            ViewBag.TransactionId = paymentResponse.TxnID;
            ViewBag.OrderType = "Invoice";
            string paymentStatus = (paymentResponse.ResponseCode == "R1000" || paymentResponse.ResponseCode == "000" || paymentResponse.ResponseCode == "0000") ? "Success" : "Failed";
            string orderId = orderDetails.OrderId ?? String.Empty;
            ViewBag.paymentStatus = paymentStatus;//string paymentStatus = "Success";
            orderDetails.paymentResponse = paymentResponse;



            using (var httpClient = Common.GetHttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(orderDetails), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "OrderMaster/CustomerOrderUpdate", stringContent))
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
            if (!String.IsNullOrEmpty(orderDetails.OrderId))
            {
                InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();
                using (var httpClient = Common.GetHttpClient())
                {
                    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + orderDetails.OrderId + "/" + orderDetails.OrderType))
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
                // Define the file path (relative to wwwroot)
                const string LogoPathAndName = "assets/images/Cloudtree_final_Logo.png";

                // 1. Pass the server's root path and the image name/path to the helper
                string base64Logo = ImageToString.GetBase64Logo(_env.WebRootPath, LogoPathAndName);

                // 2. Attach the finished string to the ViewModel
                // (Ensure your GenerateInvoiceViewModel has a Base64LogoString property)
                invoice.Base64LogoString = base64Logo;
                string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);

                // 2. CONVERT HTML STRING TO PDF BYTE ARRAY
                byte[] pdfBytes = await GeneratePdfFromHtml(invoiceHtml);

                // 3. SEND EMAIL WITH PDF ATTACHMENT
                string toEmail = orderDetails.EmailAddress ?? String.Empty;
                string contactPerson = orderDetails.ContactPersonName ?? String.Empty;

                //sendEmail(toEmail, password, toEmail, contactPerson, pdfBytes, $"Invoice_{OrderDetailViewModel.InvoiceId}.pdf");
                sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{orderDetails.InvoiceId}.pdf");
            }
            else
            {
                return RedirectToAction("Login", "CustomerManagement");
                //return View("PaymentResponse", OrderDetailViewModel);
            }



            orderDetails = new CustomerOrderViewModel();
            return View("PaymentResponse", orderId);
        }



        // Subscription Checkout Page
        public IActionResult RazorpaySubscriptionPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RazorPayPaymentInitiate1(int packCount, double packCost, string jsonData, int Csid, double totalTaxValue, double totalValue, double DiscountAmount, int dmid, string CustomerPhoneNumber, string CustomerEmail)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                OrderDetailViewModel.packCount = packCount;
                OrderDetailViewModel.packCost = packCost;
                string updatedjsonData = JsonConvert.SerializeObject(OrderDetailViewModel);
                HttpContext.Session.SetString("OrderDetailJson", updatedjsonData);
                HttpContext.Session.SetInt32("dmid", dmid);
                OrderDetailViewModel = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                int sbfid = OrderDetailViewModel.SelectedSubscription.Sbfid;
                string paymentGatewayPlanId = string.Empty;
                string razorpayOfferId = string.Empty;
                OrderDetailViewModel.PortalCustomerDetails.Csid = Csid;

                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetPaymentGatewayPlanId?sbfid={sbfid}";
                    var response = await httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        return Content("Unable to fetch Razorpay Plan ID.");
                    dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                    paymentGatewayPlanId = result?.paymentGatewayMasterId;
                    HttpContext.Session.SetString("paymentGatewayPlanId", paymentGatewayPlanId);
                }

                int days = Convert.ToInt32(OrderDetailViewModel.SelectedSubscription.NoofDays);

                double taxv = totalValue - totalTaxValue + DiscountAmount;
                double taxAmount = (taxv * OrderDetailViewModel.TaxPercentage) / 100;
                double newPlanAmount = taxv + taxAmount;

                double initialPaymentAmount = totalValue - DiscountAmount;
                if (initialPaymentAmount < 1) initialPaymentAmount = 1;
                int initialPaymentInPaise = (int)(initialPaymentAmount * 100) + (int)(DiscountAmount * 100);

                string period = "daily";
                int interval = days;
                if (days % 365 == 0)
                {
                    period = "yearly";
                    interval = days / 365;
                }
                else if (days % 30 == 0)
                {
                    period = "monthly";
                    interval = days / 30;
                }
                else if (days % 7 == 0)
                {
                    period = "weekly";
                    interval = days / 7;
                }
                else
                {
                    period = "daily";
                    interval = days;
                }
                Dictionary<string, object> planOptions = new Dictionary<string, object>
{
    { "period", period },
    { "interval", interval },
    { "item", new Dictionary<string, object>
        {
            { "name", $"Plan {days} Days - {totalValue}" },
            { "amount", newPlanAmount },
            { "currency", "INR" },
            { "description", $"Recurring {days} days plan" }
        }
    }
};

                Plan plan = client.Plan.Create(planOptions);
                string dynamicPlanId = plan["id"].ToString();
                HttpContext.Session.SetString("paymentGatewayPlanId", dynamicPlanId);
                DateTimeOffset startAtDate = DateTimeOffset.UtcNow.AddDays(days);
                long startAt = startAtDate.ToUnixTimeSeconds();
                var subscriptionOptions = new Dictionary<string, object>
{
    { "plan_id", dynamicPlanId },
    { "total_count", 120 },
    { "quantity", 1 },
    { "customer_notify", 1 },
    { "start_at", startAt },
    { "notes", new Dictionary<string, string> {


                 { "CustomerId", Csid.ToString() },
                { "CustomerName", OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName ?? "" },
                { "CustomerEmail", CustomerEmail ?? "" },
                { "PlanName", OrderDetailViewModel?.SelectedSubscription?.SubscriptionName ?? "Subscription" },
                { "PlanPeriod", $"Every {days} Days" }


    } },
    { "addons", new List<Dictionary<string, object>> {
        new Dictionary<string, object> {
            { "item", new Dictionary<string, object> {
                { "name", "Initial Payment" },
                { "amount", initialPaymentInPaise },
                { "currency", "INR" }
            }}
        }
    }}
};

                Subscription subscription = client.Subscription.Create(subscriptionOptions);
                string subscriptionId = subscription["id"].ToString();
                discountIdViewModel.RazorpaySubscriptionId = subscriptionId;
                discountIdViewModel.TransactionId = OrderDetailViewModel.OrderId;
                discountIdViewModel.PlanId = dynamicPlanId;
                discountIdViewModel.CsId = Csid;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                    await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                }

                // --- 5. VIEW MODEL FOR CHECKOUT ---
                var model = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    SubscriptionId = subscriptionId,
                    CustomerName = OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName,
                    Email = CustomerEmail,
                    Phone = CustomerPhoneNumber,
                    Amount = initialPaymentAmount
                };

                HttpContext.Session.SetString("SubscriptionJson", JsonConvert.SerializeObject(model));
                return View("RazorpaySubscriptionPage", model);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }



        [HttpPost]
        public async Task<IActionResult> RazorPayPaymentInitiate(int packCount, double packCost, string jsonData, int Csid, double totalTaxValue, double totalValue, double DiscountAmount, int dmid, string CustomerPhoneNumber, string CustomerEmail)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                OrderDetailViewModel = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                OrderDetailViewModel.packCount = packCount;
                OrderDetailViewModel.packCost = packCost;
                string updatedjsonData = JsonConvert.SerializeObject(OrderDetailViewModel);
                HttpContext.Session.SetString("OrderDetailJson", updatedjsonData);
                HttpContext.Session.SetInt32("dmid", dmid);
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                int sbfid = OrderDetailViewModel.SelectedSubscription.Sbfid;
                OrderDetailViewModel.PortalCustomerDetails.Csid = Csid;
                int days = Convert.ToInt32(OrderDetailViewModel.SelectedSubscription.NoofDays);
                double taxv = totalValue - totalTaxValue + DiscountAmount;
                double taxCalc = (taxv * OrderDetailViewModel.TaxPercentage) / 100;
                double newPlanAmount = taxv + taxCalc;
                int newPlanAmountInPaise = (int)(newPlanAmount * 100);
                double initialPaymentAmount = totalValue;
                if (initialPaymentAmount < 1) initialPaymentAmount = 1;
                int initialPaymentInPaise = (int)(initialPaymentAmount * 100);
                string finalPlanId = string.Empty;
                if (packCount > 0 && packCost > 0)
                {
                    string period = "daily";
                    int interval = days;
                    if (days % 365 == 0) { period = "yearly"; interval = days / 365; }
                    else if (days % 30 == 0) { period = "monthly"; interval = days / 30; }
                    else if (days % 7 == 0) { period = "weekly"; interval = days / 7; }
                    Dictionary<string, object> planOptions = new Dictionary<string, object>
            {
                { "period", period },
                { "interval", interval },
                { "item", new Dictionary<string, object>
                    {
                        { "name", $"Custom Plan - {days} Days" },
                        { "amount", newPlanAmountInPaise },
                        { "currency", "INR" },
                        { "description", $"Addon plan for {packCount} packs" }
                    }
                }
            };

                    Plan plan = client.Plan.Create(planOptions);
                    finalPlanId = plan["id"].ToString();
                }
                else
                {

                    using (var httpClient = Common.GetHttpClient())
                    {
                        var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetPaymentGatewayPlanId?sbfid={sbfid}";
                        var response = await httpClient.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                            return Content("Unable to fetch Razorpay Plan ID from DB.");

                        dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                        finalPlanId = result?.paymentGatewayMasterId;
                    }
                }
                HttpContext.Session.SetString("paymentGatewayPlanId", finalPlanId);
                DateTimeOffset startAtDate = DateTimeOffset.UtcNow.AddDays(days);
                long startAt = startAtDate.ToUnixTimeSeconds();
                var subscriptionOptions = new Dictionary<string, object>
        {
            { "plan_id", finalPlanId },
            { "total_count", 100 },
            { "quantity", 1 },
            { "customer_notify", 1 },
            { "start_at", startAt },
            { "notes", new Dictionary<string, string> {
                { "CustomerId", Csid.ToString() },
                { "CustomerName", OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName ?? "" },
                { "CustomerEmail", CustomerEmail ?? "" },
                { "PlanName", OrderDetailViewModel?.SelectedSubscription?.SubscriptionName ?? "Subscription" },
                { "PlanPeriod", $"Every {days} Days" }
            } },
            { "addons", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "item", new Dictionary<string, object> {
                        { "name", "Initial Payment" },
                        { "amount", initialPaymentInPaise },
                        { "currency", "INR" }
                    }}
                }
            }}
        };

                Subscription subscription = client.Subscription.Create(subscriptionOptions);
                string subscriptionId = subscription["id"].ToString();
                discountIdViewModel.RazorpaySubscriptionId = subscriptionId;
                discountIdViewModel.TransactionId = OrderDetailViewModel.OrderId;
                discountIdViewModel.PlanId = finalPlanId;
                discountIdViewModel.CsId = Csid;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                    await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                }
                var model = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    SubscriptionId = subscriptionId,
                    CustomerName = OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName,
                    Email = CustomerEmail,
                    Phone = CustomerPhoneNumber,
                    Amount = initialPaymentAmount
                };
                HttpContext.Session.SetString("SubscriptionJson", JsonConvert.SerializeObject(model));
                return View("RazorpaySubscriptionPage", model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ServiceUnavailable", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RazorpayPaymentSuccess(string razorpay_payment_id, string razorpay_subscription_id, string razorpay_signature)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                string jsonData = HttpContext.Session.GetString("OrderDetailJson") ?? "";
                string dmcode = HttpContext.Session.GetString("DMCode") ?? "";
                string PlanId = HttpContext.Session.GetString("paymentGatewayPlanId") ?? "";
                int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
                int dmid = HttpContext.Session.GetInt32("dmid") ?? 0;
                if (string.IsNullOrEmpty(jsonData))
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Session timeout. Please login and try again."
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }
                OrderDetailViewModel orderDetail = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                orderDetail.PaymentinitiatedBy = HttpContext.Session.GetString("UserName") + "@" + csid;
                orderDetail.Dmid = dmid;
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                string payload = razorpay_payment_id + "|" + razorpay_subscription_id;
                string expectedSignature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    expectedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                if (expectedSignature == razorpay_signature)
                {
                    RazorpayClient client = new RazorpayClient(key, secret);
                    Payment payment = client.Payment.Fetch(razorpay_payment_id);
                    string status = payment["status"]?.ToString() ?? "unknown";
                    string txnId = payment["id"]?.ToString() ?? "N/A";
                    // paymentResponse.TxnID = razorpay_subscription_id;
                    paymentResponse.TxnID = txnId;
                    paymentResponse.ResponseCode = (status == "authorized" || status == "captured") ? "Success" : "Failed";
                    string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/ProcessRazorpayPayment?paymentId={razorpay_payment_id}&subscriptionId={razorpay_subscription_id}&status={status}";
                    string password = string.Empty;
                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromHours(1);
                        var content = new StringContent(JsonConvert.SerializeObject(orderDetail), Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync(apiUrl, content);
                        string responseString = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(responseString);
                        password = json["password"]?.ToString();
                    }
                    if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                    {
                        InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();
                        using (var httpClient = Common.GetHttpClient())
                        {
                            var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Subscription");
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse) ?? new InvoicePrintDetailsViewModel();
                            }
                        }
                        string logo = ImageToString.GetBase64Logo(_env.WebRootPath, "assets/images/Cloudtree_final_Logo.png");
                        invoice.Base64LogoString = logo;
                        invoice.OrderNumber = OrderDetailViewModel.OrderId;
                        //string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = invoice.OrderNumber, orderType = invoice.RecordType }, Request.Scheme) ?? string.Empty;

                        string urlForQr = $"{Request.Scheme}://{Request.Host}/Order/GenerateInvoice/{orderDetail.OrderId}/{invoice.RecordType}";
                        invoice.QrCodeBase64Image = GenerateQrCodeBase64.GenerateQrCode(urlForQr);
                        string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);
                        byte[] pdfBytes = await GeneratePdfFromHtml(invoiceHtml);
                        string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? "";
                        string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? "";
                        sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceNumber}.pdf");
                        if (!string.IsNullOrEmpty(password))
                        {
                            sendEmail(orderDetail.PortalCustomerDetails?.EmailAddress ?? "", password, orderDetail.PortalCustomerDetails?.EmailAddress ?? "", orderDetail.PortalCustomerDetails?.ContactPersonName ?? "");
                        }
                        var successModel = new PaymentResponse
                        {
                            Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                            Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            OrderId = OrderDetailViewModel.OrderId,
                            PaymentID = razorpay_payment_id,
                            Status = status
                        };

                        return RedirectToAction("RazorPaymentResponse", successModel);
                    }
                }
                else
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Payment verification passed but an unexpected issue occurred. Please contact support.",
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }

                return RedirectToAction("RazorPaymentResponse");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ServiceUnavailable", "Home");
            }

        }


        public async Task<IActionResult> SubscriptionSuccessOld(string razorpay_payment_id, string razorpay_subscription_id, string razorpay_signature)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                string jsonData = HttpContext.Session.GetString("OrderDetailJson");
                string companyDetails = HttpContext.Session.GetString("newCompanyDetailsJson");
                string PlanId = HttpContext.Session.GetString("paymentGatewayPlanId");
                int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (string.IsNullOrEmpty(jsonData))
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Session timeout. Please login and try again."
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }
                OrderDetailViewModel orderDetail = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                Payment payment = client.Payment.Fetch(razorpay_payment_id);
                string status = payment["status"]?.ToString() ?? "unknown";
                string txnId = payment["id"]?.ToString() ?? "N/A";
                paymentResponse.TxnID = txnId;
                paymentResponse.ResponseCode = (status == "authorized" || status == "captured") ? "Success" : "Failed";
                string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/ProcessRazorpayPayment?paymentId={razorpay_payment_id}&subscriptionId={razorpay_subscription_id}&status={status}";
                string password = string.Empty;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(orderDetail), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    string responseString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseString);
                    password = json["password"]?.ToString();
                }
                if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                {
                    InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

                    using (var httpClient = Common.GetHttpClient())
                    {
                        var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Subscription");
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse) ?? new InvoicePrintDetailsViewModel();
                        }
                    }
                    string logo = ImageToString.GetBase64Logo(_env.WebRootPath, "assets/images/Cloudtree_final_Logo.png");
                    invoice.Base64LogoString = logo;
                    invoice.OrderNumber = OrderDetailViewModel.OrderId;
                    string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = invoice.OrderNumber, orderType = invoice.RecordType }, Request.Scheme) ?? string.Empty;
                    invoice.QrCodeBase64Image = GenerateQrCodeBase64.GenerateQrCode(urlForQr);
                    string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);
                    byte[] pdfBytes = await GeneratePdfFromHtml(invoiceHtml);
                    string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? "";
                    string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? "";
                    sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceNumber}.pdf");
                    if (!string.IsNullOrEmpty(password))
                    {
                        sendEmail(orderDetail.PortalCustomerDetails?.EmailAddress ?? "", password, orderDetail.PortalCustomerDetails?.EmailAddress ?? "", orderDetail.PortalCustomerDetails?.ContactPersonName ?? "");
                    }
                    var successModel = new PaymentResponse
                    {
                        Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                        Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Amount = Convert.ToDecimal(payment["amount"]) / 100,
                        OrderId = OrderDetailViewModel.OrderId,
                        PaymentID = razorpay_payment_id,
                        Status = status
                    };

                    return RedirectToAction("RazorPaymentResponse", successModel);
                }
                else
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Payment verification passed but an unexpected issue occurred. Please contact support.",
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }

            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }

        }

        public IActionResult RazorpayPaymentPage()
        {

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string paymentId, string orderId, string razorpay_signature, string razorpay_payment_id, string razorpay_subscription_id)
        {
            try
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                string jsonData = HttpContext.Session.GetString("OrderDetailJson");
                if (string.IsNullOrEmpty(jsonData))
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Session timeout. Please login and try again."
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }
                OrderDetailViewModel orderDetail = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                orderDetail.PaymentinitiatedBy = HttpContext.Session.GetString("UserName") + "@" + csid;

                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                string payload = orderId + "|" + paymentId;
                string expectedSignature;
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    expectedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                if (expectedSignature == razorpay_signature)
                {
                    RazorpayClient client = new RazorpayClient(key, secret);
                    Payment payment = client.Payment.Fetch(paymentId);//Payment Fetch
                    string status = payment["status"]?.ToString() ?? "unknown";
                    string txnId = payment["id"]?.ToString() ?? "N/A";
                    paymentResponse.TxnID = txnId;
                    paymentResponse.Status = (status == "captured") ? "Success" : "Failed";
                    string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/ProcessRazorpayPayment?paymentId={paymentId}&razorpay_signature={razorpay_signature}&status={status}";


                    string password = string.Empty;
                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        var content = new StringContent(JsonConvert.SerializeObject(orderDetail), Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync(apiUrl, content);
                        string responseString = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(responseString);
                        password = json["password"]?.ToString();
                    }
                    if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                    {
                        InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

                        using (var httpClient = Common.GetHttpClient())
                        {
                            var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Subscription");
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse) ?? new InvoicePrintDetailsViewModel();
                            }
                        }
                        string logo = ImageToString.GetBase64Logo(_env.WebRootPath, "assets/images/Cloudtree_final_Logo.png");
                        invoice.Base64LogoString = logo;
                        string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);
                        byte[] pdfBytes = await GeneratePdfFromHtml(invoiceHtml);
                        string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? "";
                        string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? "";
                        sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceId}.pdf");

                        // WELCOME MAIL                   
                        if (!string.IsNullOrEmpty(password))
                        {
                            sendEmail(orderDetail.PortalCustomerDetails?.EmailAddress ?? "", password, orderDetail.PortalCustomerDetails?.EmailAddress ?? "", orderDetail.PortalCustomerDetails?.ContactPersonName ?? "");
                        }
                        var successModel = new PaymentResponse
                        {
                            Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                            Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            OrderId = OrderDetailViewModel.OrderId,
                            PaymentID = paymentId,
                            Status = status
                        };

                        return RedirectToAction("RazorPaymentResponse", successModel);
                    }
                }
                else
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Payment verification passed but an unexpected issue occurred. Please contact support.",
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }

                return RedirectToAction("RazorPaymentResponse");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ServiceUnavailable", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentFailure(string orderId, string status, string errorDescription)
        {
            try
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

                string jsonData = HttpContext.Session.GetString("OrderDetailJson");
                if (string.IsNullOrEmpty(jsonData))
                {
                    var errorModel = new PaymentResponse
                    {
                        Status = "Failed",
                        ErrorMessage = "Session timeout. Please login and try again."
                    };
                    return RedirectToAction("RazorPaymentResponse", errorModel);
                }
                OrderDetailViewModel orderDetail = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                orderDetail.PaymentinitiatedBy = HttpContext.Session.GetString("UserName") + "@" + csid;

                string finalStatus = status ?? "failed";
                string paymentId = "";
                string razorpay_signature = "";
                string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/ProcessRazorpayPayment?" + $"paymentId={paymentId}&razorpay_signature={razorpay_signature}&status={finalStatus}";
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    StringContent content = new StringContent(JsonConvert.SerializeObject(orderDetail), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    string responseString = await response.Content.ReadAsStringAsync();
                }
                if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                {
                    string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty;
                    string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? String.Empty;
                    sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, null, "");
                    var successModel = new PaymentResponse
                    {
                        Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                        Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Amount = orderDetail.SelectedSubscription?.Amount ?? 0,
                        OrderId = OrderDetailViewModel.OrderId,
                        PaymentID = paymentId,
                        Status = finalStatus
                    };
                    //return RedirectToAction("PaymentSuccessPage", successModel);
                    //HttpContext.Session.Remove("OrderDetailJson");
                    return RedirectToAction("RazorPaymentResponse", successModel);
                }
                //HttpContext.Session.Remove("OrderDetailJson");
                var failureModel = new PaymentResponse
                {
                    Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                    Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                    Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                    Amount = orderDetail.SelectedSubscription?.Amount ?? 0,
                    OrderId = orderDetail.OrderId,
                    PaymentID = "N/A",
                    Status = finalStatus,
                    ErrorMessage = errorDescription ?? $"Payment was {finalStatus}."
                };

                return RedirectToAction("RazorPaymentResponse", failureModel);
            }
            catch (Exception ex)
            {
                return Content("Error during Payment Failure Processing: " + ex.Message);
            }
        }
        public IActionResult RazorPaymentResponse(PaymentResponse model)
        {

            HttpContext.Session.SetString("payment_response", JsonConvert.SerializeObject(model));
            return RedirectToAction("PaymentResponseResult");
        }
        public IActionResult PaymentResponseResult()
        {
            var json = HttpContext.Session.GetString("payment_response");
            if (json == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            var model = JsonConvert.DeserializeObject<PaymentResponse>(json);
            ViewBag.TransactionId = model.OrderId;
            ViewBag.OrderType = "Subscription";
            ViewBag.paymentStatus = (model.Status == "captured" || model.Status == "authorized") ? "Success" : "Failed";
            return View("RazorPaymentResponse", model);
        }




    }



}
