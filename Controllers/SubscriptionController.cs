using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NReco.PdfGenerator;
using NuGet.Versioning;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Razorpay.Api;
using System.Net.Mail;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using static QuestPDF.Helpers.Colors;
using static System.Net.Mime.MediaTypeNames;

namespace HCMPortal.Controllers
{
    public class SubscriptionController : Controller
    {
        private CommonServices commonservice = new CommonServices();
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private PaymentResponse paymentResponse = new PaymentResponse();
        public SubscriptionController(IWebHostEnvironment env, IConfiguration config)
        {

            _env = env;
            _config = config;
        }
        /// <summary>
        /// this is the suscription details page
        /// </summary>
        /// <returns></returns>
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        //[HttpGet("hcm-solution/subscription/subscription-details",Name = "SubscriptionDetails")]
        [HttpGet("hcm-solution/subscription-details",Name = "SubscriptionDetails", Order = 0)]
        public async Task<IActionResult> SubscriptionDetails()
        {
            SubscriptionPageViewModel suscriptionPageViewModel = new SubscriptionPageViewModel();

            suscriptionPageViewModel.Plans = await commonservice.getAllPlans();
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
            suscriptionPageViewModel.SubscriptionDetails = subscriptionDetails;
            SubscriptionDetailsViewModel trialPlanDetails = new SubscriptionDetailsViewModel();
            trialPlanDetails = subscriptionDetails.FirstOrDefault(e => e.ValidUptoDays != null && e.ValidUptoDays >= 0) ?? new SubscriptionDetailsViewModel();
            ViewBag.TrialPlan = trialPlanDetails;
            subscriptionDetails.RemoveAll(e => e.SubscriptionName == trialPlanDetails.SubscriptionName);
            subscriptionDetails.Add(trialPlanDetails);
            if (subscriptionDetails.Any(e => suscriptionPageViewModel.Plans.Any(x => x.PlanId == e.Bid) && e.Amount == 0))
            {
                List<int> planId = subscriptionDetails
    // Group all plans by their Bid ID
    .GroupBy(x => x.Bid)
    // Filter the groups: we only want the groups where ALL plans have Amount == 0
    .Where(g => g.All(p => p.Amount == 0))
    // Select just the distinct Bid ID from these groups
    .Select(g => g.Key)
    .ToList();
                foreach (var item in planId)
                {
                    if (subscriptionDetails.DistinctBy(e => new { e.Sid, item }).ToList().Any(e => e.Amount == 0))
                    {
                        PlanMaster plan = suscriptionPageViewModel.Plans.FirstOrDefault(e => e.PlanId == item) ?? new PlanMaster();
                        suscriptionPageViewModel.Plans.Remove(plan);
                    }
                }
            }


            // ====================== TESTIMONIAL LIST  Daitari(25-11-2025)======================
            List<TestimonialModel> testimonialList = new List<TestimonialModel>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Testimonial/GetAllTestimonialList"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testimonialList = JsonConvert.DeserializeObject<List<TestimonialModel>>(apiResponse) ?? new List<TestimonialModel>();
                    }
                }
            }
            foreach (var item in testimonialList)
            {
                if (String.IsNullOrEmpty(item.pimage))
                {
                    item.pimage = Common.ImageAndDocSavePageAddress().Replace("http://", "https://").Replace("/api/", "/") + "assets/UploadImg/27102025153016.png";
                }
                else
                {
                    string relativePath = Path.Combine("assets", "UploadImg", item.pimage);
                    string absolutePath = Path.Combine(_env.WebRootPath, relativePath);
                    bool fileExists = System.IO.File.Exists(absolutePath);
                    if (fileExists)
                    {
                        item.pimage = $"/{relativePath}";
                    }
                    else
                    {
                        item.pimage = Common.ImageAndDocSavePageAddress().Replace("http://", "https://").Replace("/api/", "/") + "assets/UploadImg/" + item.pimage;
                    }
                }
            }
            suscriptionPageViewModel.testimonialList = testimonialList;
            ViewBag.SubscriptionWarning = TempData["SubscriptionWarning"];
            
            return View(suscriptionPageViewModel);

        }

        [HttpGet("customer/mysubscription",Name = "MySubscriptionDetails")]
        public async Task<IActionResult> MySubscriptionDetails()
        {
            CustomerSubscriptionDetailsViewModel customerSubscriptionDetailsViewModel = new CustomerSubscriptionDetailsViewModel();
            List<CustomerSubscriptionDetailsViewModel> customerSubscriptionDetails = new List<CustomerSubscriptionDetailsViewModel>();
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetCustomerSubscriptionDetails/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        customerSubscriptionDetails = JsonConvert.DeserializeObject<List<CustomerSubscriptionDetailsViewModel>>(apiResponse) ?? new List<CustomerSubscriptionDetailsViewModel>();


                    }
                }
            }
            List<CancelSubscriptionDetailsViewModel> cancelList = new List<CancelSubscriptionDetailsViewModel>();
            cancelList = (await commonservice.CancelationRequestList()).Where(e => e.Csid == csid).ToList();
            foreach (var item in customerSubscriptionDetails)
            {
                if (cancelList.Any(e => e.MembershipId == item.MembershipId))
                {
                    var data = cancelList.FirstOrDefault(e => e.MembershipId == item.MembershipId);
                    if (data.IsDetele == 1)
                    {
                        item.IsRequestAccepted = true;

                    }
                    else if (data.Flag == 2 && DateTime.Now.Date >= data.CreatedDate.Date)
                    {
                        item.IsRequestRejected = true;
                    }
                    item.IsRequestForCancelation = true;
                }

            }
            TblTaxMaster taxMaster = await commonservice.getTaxdetails();
            ViewBag.TaxPercentage = taxMaster.TaxPercentage;

            return View(customerSubscriptionDetails);
        }


        public async Task<IActionResult> DowngradeSubscription()
        {
            return View();
        }


        public async Task<IActionResult> GetUpgradeOptionsPartial()
        {
            SubscriptionPageViewModel suscriptionPageViewModel = new SubscriptionPageViewModel();

            suscriptionPageViewModel.Plans = await commonservice.getAllPlans();
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
            suscriptionPageViewModel.SubscriptionDetails = subscriptionDetails;
            SubscriptionDetailsViewModel trialPlanDetails = new SubscriptionDetailsViewModel();
            trialPlanDetails = subscriptionDetails.FirstOrDefault(e => e.ValidUptoDays != null && e.ValidUptoDays >= 0) ?? new SubscriptionDetailsViewModel();
            ViewBag.TrialPlan = trialPlanDetails;
            subscriptionDetails.RemoveAll(e => e.SubscriptionName == trialPlanDetails.SubscriptionName);
            subscriptionDetails.Add(trialPlanDetails);
            if (subscriptionDetails.Any(e => suscriptionPageViewModel.Plans.Any(x => x.PlanId == e.Bid) && e.Amount == 0))
            {
                List<int> planId = subscriptionDetails
    // Group all plans by their Bid ID
    .GroupBy(x => x.Bid)
    // Filter the groups: we only want the groups where ALL plans have Amount == 0
    .Where(g => g.All(p => p.Amount == 0))
    // Select just the distinct Bid ID from these groups
    .Select(g => g.Key)
    .ToList();
                foreach (var item in planId)
                {
                    if (subscriptionDetails.DistinctBy(e => new { e.Sid, item }).ToList().Any(e => e.Amount == 0))
                    {
                        PlanMaster plan = suscriptionPageViewModel.Plans.FirstOrDefault(e => e.PlanId == item) ?? new PlanMaster();
                        suscriptionPageViewModel.Plans.Remove(plan);
                    }
                }
            }
            return PartialView("_UpgradePlansPartial", suscriptionPageViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpgradeSubscription(int orderId, int renewalId, string membershipid)
        {
            HttpContext.Session.SetString("membershipid", membershipid ?? string.Empty);
            CustomerSubscriptionDetailsViewModel customerSubscriptionDetailsViewModel = new CustomerSubscriptionDetailsViewModel();
            int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            using (var httpClient = Common.GetHttpClient())
            {
                //using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetSubscriptionDetailsForUpgrade/" + orderId + "/" + renewalId))
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetSubscriptionDetailsForUpgrade/" + orderId + "/" + renewalId + "/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        customerSubscriptionDetailsViewModel = JsonConvert.DeserializeObject<CustomerSubscriptionDetailsViewModel>(apiResponse) ?? new CustomerSubscriptionDetailsViewModel();
                    }
                }
            }
            ViewBag.PlanList = await commonservice.getAllPlans();
            List<SubscriptionDetailsViewModel> subscriptions = await commonservice.GetAllSubscriptionMaster();

            if (customerSubscriptionDetailsViewModel.SusbcriptionName == "Classic")
            {
                subscriptions.RemoveAll(x => x.SubscriptionName == "Basic");
            }
            else if (customerSubscriptionDetailsViewModel.SusbcriptionName == "Enterprise")
            {
                subscriptions.RemoveAll(x => x.SubscriptionName == "Basic");
                subscriptions.RemoveAll(x => x.SubscriptionName == "Classic");
            }
            ViewBag.SubscriptionList = subscriptions?
      .Where(x => !x.SubscriptionName.Equals("Trial", StringComparison.OrdinalIgnoreCase))
      .ToList();


            return View(customerSubscriptionDetailsViewModel);
        }
        public async Task<IActionResult> DowngradeSubscriptionreason()
        {
            return View();
        }



        //public IActionResult CancelSubscription(int Sid,int csid, int csorderId, string planName,string membershipid, string billingCycle, double amount, string createdDate, string nextRenewalDate)
        public IActionResult CancelSubscription(int csid, string membershipid)
        {
            try
            {
                var model = new CustomerSubscriptionDetailsViewModel
                {
                    Csid = csid,
                    MembershipId = membershipid
                };
                return View(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public async Task<IActionResult> CancelSubscription(CustomerSubscriptionDetailsViewModel model)
        {
            if (model == null || model.Csid == 0)
                return Json(new { success = false, message = "Invalid request" });

            try
            {
                int csid = model.Csid;
                string membershipId = model.MembershipId;
                string cancelReason = model.CancelReason;

                // Build query string
                var url = Common.GetBaseAddress() + $"SubscriptionMaster/CancelCustomerSubscription?csid={csid}&cancelReason={Uri.EscapeDataString(cancelReason)}&membershipId={Uri.EscapeDataString(membershipId)}";

                using var httpClient = Common.GetHttpClient();

                // Send POST request with empty body since parameters are in URL
                var response = await httpClient.PostAsync(url, null);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"API call failed: {err}" });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> GetSubscriptionDetails(int sid, int bid)
        {

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
            SubscriptionDetailsViewModel subscription = subscriptionDetails.Where(e => e.Sid == sid && e.Bid == bid).FirstOrDefault() ?? new SubscriptionDetailsViewModel();

            return Json(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlansBySubscription(int subscriptionId)
        {
            List<PlanMaster>? Plans = new List<PlanMaster>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "AMSPlanMaster/GetPlansBySubscriptionId/" + subscriptionId))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        Plans = JsonConvert.DeserializeObject<List<PlanMaster>>(apiResponse);
                        Plans = (Plans != null) ? Plans.OrderByDescending(e => e.PlanId).ToList() : Plans;

                    }
                    else
                    {
                        string errpr = await response.Content.ReadAsStringAsync();
                    }
                }
            }

            return Json(Plans);
        }


        public async Task<OrderDetailViewModel> PrepareOrderDetails(int sid, int bid)
        {

            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel();
            orderDetailViewModel.OrderId = commonservice.GenerateOrderNumber();
            TblTaxMaster taxMaster = new TblTaxMaster();
            taxMaster = await commonservice.getTaxdetails();

            List<PlanMaster>? PlanMaster = await commonservice.getAllPlans();
            orderDetailViewModel.ListOfPlanMater = PlanMaster;
            orderDetailViewModel.planMaster = PlanMaster.Where(e => e.PlanId == bid).FirstOrDefault();
            orderDetailViewModel.TaxPercentage = (float)(taxMaster != null ? taxMaster.TaxPercentage : 0);
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();

            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
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
            subscriptionDetails = subscriptionDetails.Where(x => x.Bid == bid).ToList();
            if (sid == 0)
            {
                sid = subscriptionDetails.Where(x => x.IsDelete == 0).Select(x => x.Sid).FirstOrDefault();
                // orderDetailViewModel.SelectedSubscription = subscriptionDetails.FirstOrDefault();
            }
            orderDetailViewModel.SelectedSubscription = subscriptionDetails.Where(x => x.Sid == sid).FirstOrDefault();
            orderDetailViewModel.SubscriptionDetails = subscriptionDetails.Where(x => x.Sid != sid).ToList();

            TblMyCart tblMyCart = new TblMyCart();
            tblMyCart.UserId = portalCredentialViewModel.Csid;
            tblMyCart.OrderNo = orderDetailViewModel.OrderId;
            tblMyCart.Sid = sid;
            tblMyCart.Pid = bid;
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
            return orderDetailViewModel;
        }

        [HttpGet]

        public IActionResult GetCartSummary()
        {
            // Executes MyCartViewComponent and returns its rendered HTML
            return ViewComponent("MyCart");
        }




        // The Merchant ID for testing
        private readonly string _merchantId = "T_03342";
        //private readonly string _merchantId = "T_08886";

        // !!! CRITICAL: Replace "abc" with the actual Merchant Secret Key for T_03342 !!!
        private const string MerchantSecretKey = "abc";

        private static OrderDetailViewModel OrderDetailViewModel = new OrderDetailViewModel();
        [HttpPost]

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
        static string orderId = "";
        public async Task<IActionResult> PaymentResponse(PaymentResponse paymentResponse)
        {
            //comment it when realtime payment PAI integration will be done-Anshuman 24.10.2025
            paymentResponse = new PaymentResponse()
            {
                TxnID = "126437141724",
                ResponseCode = "000",
                RespDescription = "Success",
            };
            ResponseModel responseModel = new ResponseModel();
            ViewBag.TransactionId = paymentResponse.TxnID;
            ViewBag.OrderType = (String.IsNullOrEmpty(OrderDetailViewModel.RenewalMembershipId)) ? "Subscription" : "Renewal";
            string paymentStatus = (paymentResponse.ResponseCode == "R1000" || paymentResponse.ResponseCode == "000" || paymentResponse.ResponseCode == "0000") ? "Success" : "Failed";
            orderId = OrderDetailViewModel.OrderId ?? orderId;
            ViewBag.paymentStatus = paymentStatus;//string paymentStatus = "Success";



            //if (paymentStatus == "Success")
            //{
            OrderDetailViewModel.paymentResponse = paymentResponse;
            using (var httpClient = Common.GetHttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/SubscriptionRenewal", stringContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiresponse = await response.Content.ReadAsStringAsync();

                        responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            //if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
            if (responseModel.IsSuccess && !String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
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
                string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = invoice.OrderNumber, orderType = invoice.RecordType }, Request.Scheme) ?? String.Empty;
                invoice.QrCodeBase64Image = GenerateQrCodeBase64.GenerateQrCode(urlForQr);
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
    <title>Subscription Renewal Successful</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
        /* IMPORTANT: Email clients often ignore <style> blocks for buttons, 
           so inline styles are used directly on the anchor tag below. */
        
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
            background: #28a745; /* Green for Success */
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
            <h1>Subscription Successfully Renewed!</h1>
        </div>
        <div class=""email-body"">
            <p>Dear **{ContactPerson}**</p>
            <p>We are happy to confirm that your **HCM subscription** has been successfully renewed! Your service is now secured for the next billing cycle, ensuring uninterrupted access to all platform features.</p>
            
            
            
            <p style=""text-align:center; margin: 25px 0;"">
                <a href=""{url}"" 
                   style=""
                       display: inline-block; 
                       background-color: #556ee6; 
                       color: #ffffff; 
                       text-decoration: none; 
                       padding: 10px 20px; 
                       border-radius: 4px; 
                       font-weight: bold;
                       font-size: 14px;
                       border: 1px solid #556ee6; 
                       mso-padding-alt: 0; /* Outlook fix */
                       text-align: center;
                   "">
                   View Subscription Details
                </a>
            </p>
            
            <p>You can manage your subscription, view payment history, and update your details by logging into your account.</p>
            
            <p>Your official **invoice has been attached** to this email for your records.</p>
            
            <p>Thank you for continuing to choose **HCM**!</p>
            
            <p>If you have any questions or concerns regarding your renewal, please contact our support team at <a href=""mailto:info@thecloudtree.ai"">info@thecloudtree.ai</a>.</p>
        </div>
        <div class=""email-footer"">
            <p>Best regards,<br>thecloudTree.ai<br>HCM</p>
            <p>info@thecloudtree.ai Ph : (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>
";

            //< p > **Renewal Details: **</ p >
            //< ul >
            //    < li > **Plan:** { PlanName} (Optional) </ li >
            //    < li > **Next Renewal Date:** { NextRenewalDate}</ li >
            //    < li > **Amount Paid: ** { AmountPaid} (Optional) </ li >
            //    < li > **Username:** { UserName}</ li >
            //</ ul >

            // Pass the attachment data and filename to the updated EmailHelper
            EmailHelper.sendEmail(ToEmail, body, "Welcome to HCM!", null, string.Empty);
            //EmailHelper.sendEmail(ToEmail, "Welcome to HCM!", body);



        }



        public void UpgradeEmailWithInvoice(string ToEmail, string ContactPerson, PaymentResponse paymentResponse, byte[] attachmentData, string attachmentFileName, decimal currentAmount, double nextRenewalAmount)
        {
            string transactionId = paymentResponse.TxnID ?? "N/A";
            string orderId = OrderDetailViewModel.OrderId ?? string.Empty;
            string userKey = APIEncryptionHelper.Encrypt(ToEmail);
            string url = Common.GetLandingPageAddress();

            string headerColor = "#28a745";
            string headerTitle = "Subscription Upgrade Successful!";
            string mainMessage = $@"Congratulations! Your subscription has been successfully upgraded. 
                            Your payment of ₹{currentAmount} has been applied, and your next renewal amount will be ₹{nextRenewalAmount}.";

            string messageBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Subscription Upgrade</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f9f9f9; margin: 0; padding: 0; }}
        .btn{{ color: #fff; background-color: #556ee6; border-color: #556ee6; padding: .47rem .75rem; font-size: .8125rem; border-radius: .25rem; }}
        .email-container {{ max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; }}
        .email-header {{ background: {headerColor}; color: #ffffff; padding: 20px; text-align: center; }}
        .email-body {{ padding: 20px; }}
        .highlight {{ background-color: #e2f0e6; padding: 10px; border-radius: 4px; border-left: 5px solid {headerColor}; margin: 20px 0; }}
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
                <p>Current Payment: <b>₹{currentAmount}</b></p>
                <p>Next Renewal Amount: <b>₹{nextRenewalAmount}</b></p>
            </div>

            <p>We've attached your <b>Official Invoice (PDF)</b> to this email.</p>

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

            // Send email only for success
            EmailHelper.sendEmail(ToEmail, messageBody, "Subscription Upgrade Successful", attachmentData, attachmentFileName);
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
            string urlForQr = Url.Action("GenerateInvoice", "Order", new { orderId = invoice.OrderNumber, orderType = invoice.RecordType }, Request.Scheme) ?? String.Empty;
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

        public async Task<IActionResult> GetSubscriptionDetailsForRenewal(int CsOrderId, int RenewalId)
        {
            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel();
            SubscriptionDetailsViewModel subscriptionDetailsViewModel = new SubscriptionDetailsViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SubscriptionMaster/GetCurrentActiveSubscriptionDetails/" + CsOrderId + "/" + RenewalId))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        subscriptionDetailsViewModel = JsonConvert.DeserializeObject<SubscriptionDetailsViewModel>(apiResponse) ?? new SubscriptionDetailsViewModel();
                    }
                }
            }

            orderDetailViewModel = await PrepareOrderDetailsForrenewal(subscriptionDetailsViewModel.Sid, subscriptionDetailsViewModel.Bid);
            orderDetailViewModel.RenewalMembershipId = subscriptionDetailsViewModel.ActiveMembershipId;
            //orderDetailViewModel.RecordType = subscriptionDetailsViewModel.ActiveMembershipId;
            return PartialView("_RenewalOrderDetails", orderDetailViewModel);
        }
        public async Task<OrderDetailViewModel> PrepareOrderDetailsForrenewal(int sid, int bid)
        {

            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel();
            orderDetailViewModel.OrderId = commonservice.GenerateOrderNumber();
            TblTaxMaster taxMaster = new TblTaxMaster();
            taxMaster = await commonservice.getTaxdetails();
            List<PlanMaster>? PlanMaster = await commonservice.getAllPlans();
            orderDetailViewModel.ListOfPlanMater = PlanMaster;
            orderDetailViewModel.planMaster = PlanMaster.Where(e => e.PlanId == bid).FirstOrDefault();
            orderDetailViewModel.TaxPercentage = (float)(taxMaster != null ? taxMaster.TaxPercentage : 0);
            PortalCredentialViewModel portalCredentialViewModel = new PortalCredentialViewModel();

            int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
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
            subscriptionDetails = subscriptionDetails.Where(x => x.Bid == bid).ToList();
            if (sid == 0)
            {
                sid = subscriptionDetails.Where(x => x.IsDelete == 0).Select(x => x.Sid).FirstOrDefault();
                // orderDetailViewModel.SelectedSubscription = subscriptionDetails.FirstOrDefault();
            }
            orderDetailViewModel.SelectedSubscription = subscriptionDetails.Where(x => x.Sid == sid).FirstOrDefault();
            orderDetailViewModel.SubscriptionDetails = subscriptionDetails.Where(x => x.Sid != sid).ToList();


            return orderDetailViewModel;
        }

     
        public IActionResult RazorpayPaymentPage()
        {

            return View();
        }

        public async Task<IActionResult> PaymentInitiate(string jsonData, int Csid, double totalValue, double DiscountAmount, int dmid, string CustomerPhoneNumber, string CustomerEmail, string orderId, double totalBaseAmount)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                HttpContext.Session.SetString("BaseAmount", totalBaseAmount.ToString());
                HttpContext.Session.SetString("OrderDetailJson", jsonData);
                OrderDetailViewModel = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                int subscriptionDays = Convert.ToInt32(OrderDetailViewModel.SelectedSubscription.NoofDays);
                int safeDays = subscriptionDays > 0 ? subscriptionDays : 30;
                long startAt = DateTimeOffset.UtcNow.AddDays(safeDays).ToUnixTimeSeconds();
                int sbfid = OrderDetailViewModel.SelectedSubscription.Sbfid;
                string paymentGatewayPlanId = string.Empty;
                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetPaymentGatewayPlanId?sbfid={sbfid}";
                    var response = await httpClient.GetAsync(url);
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(apiResponse);
                    paymentGatewayPlanId = result.paymentGatewayMasterId;
                    HttpContext.Session.SetString("paymentGatewayPlanId", paymentGatewayPlanId);
                }
                var createOptions = new Dictionary<string, object>
        {
            { "plan_id", paymentGatewayPlanId },
            { "customer_notify", 1 },
            { "total_count", 120 },
            { "quantity", 1 },
            { "start_at", startAt },
            {
                "notes", new Dictionary<string, string>
                {
                    { "CustomerName", OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName },
                    { "CustomerEmail", CustomerEmail },
                    { "CustomerId", Csid.ToString() },
                    { "ActionType", "Renewal" }
                }
            },
            { "addons", new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "item", new Dictionary<string, object> {
                        { "name", "Subscription Renewal Fee" },
                        { "amount", (int)(totalValue * 100) },
                        { "currency", "INR" }
                    }}
                }
            }}
        };

                Subscription subscription = client.Subscription.Create(createOptions);
                string subscriptionId = subscription["id"].ToString();
                discountIdViewModel.RazorpaySubscriptionId = subscriptionId;
                discountIdViewModel.CsId = Csid;
                discountIdViewModel.PlanId = paymentGatewayPlanId;             
                discountIdViewModel.TransactionId = OrderDetailViewModel.OrderId;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                    string responseString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseString);
                }
                var model = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    OrderId = subscriptionId,
                    CustomerName = OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName,
                    Email = CustomerEmail,
                    Phone = CustomerPhoneNumber,
                    Amount = totalValue,
                    IsSubscription = true
                };
                HttpContext.Session.SetString("SubscriptionJson", JsonConvert.SerializeObject(model));
                return View("RazorpayPaymentPage", model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ServiceUnavailable", "Home");
            }
        }



        private async Task<string> CancelRazorpaySubscription(string subscriptionId)
        {
            try
            {
                var key = _config["Razorpay:Key"];
                var secret = _config["Razorpay:Secret"];
                using var httpClient = Common.GetHttpClient();
                var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{key}:{secret}"));
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                var url = $"https://api.razorpay.com/v1/subscriptions/{subscriptionId}/cancel";
                var response = await httpClient.PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return "Your previous subscription has been canceled successfully.";
                }
                else
                {
                    if (responseContent.Contains("subscription_already_cancelled"))
                    {
                        return "This subscription is already canceled.";
                    }
                    else
                    {
                        return "Subscription cancel failed, please check manually.";
                    }
                }
            }
            catch
            {
                return "Subscription cancel failed due to an error.";
            }
        }


        public async Task<IActionResult> SubscriptionSuccess(string razorpay_payment_id, string razorpay_subscription_id, string razorpay_signature)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                double baseAmount = Convert.ToDouble(HttpContext.Session.GetString("BaseAmount") ?? "0");
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                string PlanId = HttpContext.Session.GetString("paymentGatewayPlanId");
                ResponseModel responseModel = new ResponseModel();
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
                    Payment payment = client.Payment.Fetch(razorpay_payment_id);//Payment Fetch
                    string status = payment["status"]?.ToString() ?? "unknown";
                    string txnId = payment["id"]?.ToString() ?? "N/A";
                    // Amount comes in paise
                    decimal amountInPaise = Convert.ToDecimal(payment["amount"] ?? 0);
                    decimal amountInRupees = amountInPaise / 100; // ₹ value
                    paymentResponse.TxnID = txnId;
                    paymentResponse.ResponseCode = (status == "authorized" || status == "captured") ? "Success" : "Failed";
                    paymentResponse.Amount = (decimal?)baseAmount;
                    paymentResponse.MerchantId = csid.ToString();

                    string membershipid = HttpContext.Session.GetString("membershipid") ?? "";
                    paymentResponse.MerchantTxnNo = membershipid;
                    //OrderDetailViewModel = (String.IsNullOrEmpty(OrderDetailViewModel.RenewalMembershipId)) ? "Subscription" : "Renewal";
                    OrderDetailViewModel.paymentResponse = paymentResponse;
                    discountIdViewModel.CsId = csid;
                    discountIdViewModel.RazorpaySubscriptionId = razorpay_subscription_id;
                    discountIdViewModel.TransactionId = txnId;
                    discountIdViewModel.Status = status;
                    discountIdViewModel.PlanId = PlanId;

                    Subscription subscription = client.Subscription.Fetch(razorpay_subscription_id);
                    long? startUnix = subscription["current_start"] != null ? (long?)subscription["current_start"] :
                        subscription["start_at"] != null ? (long?)subscription["start_at"] : null;
                    long? endUnix = subscription["current_end"] != null ? (long?)subscription["current_end"] :
                        subscription["end_at"] != null ? (long?)subscription["end_at"] : null;
                    DateTime? subscriptionStartDate = null;
                    DateTime? subscriptionEndDate = null;
                    if (startUnix.HasValue)
                    {
                        subscriptionStartDate = DateTimeOffset.FromUnixTimeSeconds(startUnix.Value).ToLocalTime().DateTime;
                    }
                    if (endUnix.HasValue)
                    {
                        subscriptionEndDate = DateTimeOffset.FromUnixTimeSeconds(endUnix.Value).ToLocalTime().DateTime;
                    }
                    discountIdViewModel.AvailableFromDate = subscriptionStartDate;
                    discountIdViewModel.AvailableTo = subscriptionEndDate;
                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromHours(1);
                        var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                        string responseString = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(responseString);
                    }

                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/SubscriptionRenewal", stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiresponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                            }
                        }
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

                        //for send the notification about the renewal to the HCM portal dashboard--Anshuman 25.12.2025
                        await commonservice.SaveHCMPortalNotification(new TblCrmcommunicationTransaction()
                        {
                            QueryType = "General",
                            TransactionType = "Notification",
                            Subject = "Invoice for your recent subscription renewal",
                            Message = $"Invoice ID: {invoice.InvoiceId} for Order ID: {OrderDetailViewModel.OrderId} has been generated and sent to the customer.",
                            SendBy = HttpContext.Session.GetString("UserName") + "@" + csid,
                            SendDate = DateTime.Now,
                            MessageFrom = HttpContext.Session.GetString("Emailaddress") ?? string.Empty,
                            MessageTo = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty
                        });

                        var successModel = new PaymentResponse
                        {
                            Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                            Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            OrderId = OrderDetailViewModel.OrderId,
                            PaymentID = razorpay_payment_id,
                            Status = status,
                            OrderType = "Renewal"
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




            //try
            //{
            //    RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
            //    string jsonData = HttpContext.Session.GetString("OrderDetailJson");
            //    string PlanId = HttpContext.Session.GetString("paymentGatewayPlanId");
            //    int csid = HttpContext.Session.GetInt32("UserId") ?? 0;
            //    if (string.IsNullOrEmpty(jsonData))
            //    {
            //        var errorModel = new PaymentResponse
            //        {
            //            Status = "Failed",
            //            ErrorMessage = "Session timeout. Please login and try again."
            //        };
            //        return RedirectToAction("RazorPaymentResponse", errorModel);
            //    }
            //    OrderDetailViewModel orderDetail = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
            //    string key = _config["Razorpay:Key"];
            //    string secret = _config["Razorpay:Secret"];
            //    string payload = razorpay_payment_id + "|" + razorpay_subscription_id;
            //    string expectedSignature;
            //    using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            //    {
            //        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            //        expectedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            //    }
            //    if (expectedSignature == razorpay_signature)
            //    {
            //        RazorpayClient client = new RazorpayClient(key, secret);
            //        Payment payment = client.Payment.Fetch(razorpay_payment_id);
            //        string status = payment["status"]?.ToString() ?? "unknown";
            //        string txnId = payment["id"]?.ToString() ?? "N/A";
            //        // paymentResponse.TxnID = razorpay_subscription_id;
            //        paymentResponse.TxnID = txnId;
            //        paymentResponse.Status = (status == "captured") ? "Success" : "Failed";
            //        discountIdViewModel.CsId = csid;
            //        discountIdViewModel.RazorpaySubscriptionId = razorpay_subscription_id;
            //        discountIdViewModel.TransactionId = txnId;
            //        discountIdViewModel.Status = status;
            //        discountIdViewModel.PlanId = PlanId;

            //        if (status == "captured")
            //        {
            //            string oldSubscriptionId = HttpContext.Session.GetString("OldSubscriptionId");

            //            if (!string.IsNullOrWhiteSpace(oldSubscriptionId))
            //            {
            //                await CancelRazorpaySubscription(oldSubscriptionId);
            //            }
            //        }
            //        using (var httpClient = new HttpClient())
            //        {
            //            httpClient.Timeout = TimeSpan.FromHours(1);
            //            var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
            //            var response = await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
            //            string responseString = await response.Content.ReadAsStringAsync();
            //            var json = JObject.Parse(responseString);
            //        }
            //        string apiUrl = $"{Common.GetBaseAddress()}CustomerCompanySetup/ProcessRazorpayPayment?paymentId={razorpay_payment_id}&subscriptionId={razorpay_subscription_id}&status={status}";
            //        string password = string.Empty;
            //        using (var httpClient = new HttpClient())
            //        {
            //            httpClient.Timeout = TimeSpan.FromHours(1);
            //            var content = new StringContent(JsonConvert.SerializeObject(orderDetail), Encoding.UTF8, "application/json");
            //            var response = await httpClient.PostAsync(apiUrl, content);
            //            string responseString = await response.Content.ReadAsStringAsync();
            //            var json = JObject.Parse(responseString);
            //            password = json["password"]?.ToString();
            //        }
            //        if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
            //        {
            //            InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

            //            using (var httpClient = Common.GetHttpClient())
            //            {
            //                var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Subscription");
            //                if (response.IsSuccessStatusCode)
            //                {
            //                    string apiResponse = await response.Content.ReadAsStringAsync();
            //                    invoice = JsonConvert.DeserializeObject<InvoicePrintDetailsViewModel>(apiResponse) ?? new InvoicePrintDetailsViewModel();
            //                }
            //            }
            //            string logo = ImageToString.GetBase64Logo(_env.WebRootPath, "assets/images/Cloudtree_final_Logo.png");
            //            invoice.Base64LogoString = logo;
            //            string invoiceHtml = await RenderViewToStringAsync("InvoiceAttachment", invoice);
            //            byte[] pdfBytes = GeneratePdfFromHtml(invoiceHtml);
            //            string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? "";
            //            string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? "";
            //            sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceId}.pdf");
            //            if (!string.IsNullOrEmpty(password))
            //            {
            //                sendEmail(orderDetail.PortalCustomerDetails?.EmailAddress ?? "", password, orderDetail.PortalCustomerDetails?.EmailAddress ?? "", orderDetail.PortalCustomerDetails?.ContactPersonName ?? "");
            //            }
            //            var successModel = new PaymentResponse
            //            {
            //                Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
            //                Email = orderDetail.PortalCustomerDetails?.EmailAddress,
            //                Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
            //                Amount = Convert.ToDecimal(payment["amount"]) / 100,
            //                OrderId = OrderDetailViewModel.OrderId,
            //                PaymentID = razorpay_payment_id,
            //                Status = status
            //            };

            //            return RedirectToAction("RazorPaymentResponse", successModel);
            //        }
            //    }
            //    else
            //    {
            //        var errorModel = new PaymentResponse
            //        {
            //            Status = "Failed",
            //            ErrorMessage = "Payment verification passed but an unexpected issue occurred. Please contact support.",
            //        };
            //        return RedirectToAction("RazorPaymentResponse", errorModel);
            //    }

            //    return RedirectToAction("RazorPaymentResponse");
            //}
            //catch (Exception ex)
            //{
            //    return Content("Error: " + ex.Message);
            //}

        }
        public async Task<IActionResult> NormalPaymentInitiateold(int packCount, double packCost, string jsonData, int Csid, double totalValue, string orderId, string planId, string customerEmail, string customerPhone, double totalBaseAmount)
        {
            try
            {
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                //HttpContext.Session.SetString("OrderDetailJson", jsonData);
                HttpContext.Session.SetString("OrderID", orderId);
                HttpContext.Session.SetString("BaseAmount", totalBaseAmount.ToString());
                OrderDetailViewModel = JsonConvert.DeserializeObject<OrderDetailViewModel>(jsonData) ?? new OrderDetailViewModel();
                OrderDetailViewModel.packCost = packCost;
                OrderDetailViewModel.packCount = packCount;
                string updatedjsonData = JsonConvert.SerializeObject(OrderDetailViewModel);
                HttpContext.Session.SetString("OrderDetailJson", updatedjsonData);

                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                int subscriptionDays = Convert.ToInt32(OrderDetailViewModel.SelectedSubscription.NoofDays);
                int safeDays = subscriptionDays > 0 ? subscriptionDays : 30;
                long nextDueTimestamp = DateTimeOffset.UtcNow.AddDays(safeDays).ToUnixTimeSeconds();
                int sbfid = OrderDetailViewModel.SelectedSubscription.Sbfid;
                string paymentGatewayPlanId = string.Empty;
                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetPaymentGatewayPlanId?sbfid={sbfid}";
                    var response = await httpClient.GetAsync(url);
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(apiResponse);
                    paymentGatewayPlanId = result.paymentGatewayMasterId;
                    HttpContext.Session.SetString("paymentGatewayPlanId", paymentGatewayPlanId);
                }
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("plan_id", paymentGatewayPlanId);
                options.Add("total_count", 120);
                options.Add("quantity", 1);
                options.Add("customer_notify", 1);
                options.Add("start_at", nextDueTimestamp);
                if (totalValue > 0)
                {
                    List<Dictionary<string, object>> addons = new List<Dictionary<string, object>>();
                    Dictionary<string, object> addonItem = new Dictionary<string, object>();
                    addonItem.Add("item", new Dictionary<string, object> {
                { "name", "Upgrade Adjustment Fee" },
                { "amount", Convert.ToInt32(totalValue * 100) },
                { "currency", "INR" }
            });
                    addons.Add(addonItem);
                    options.Add("addons", addons);
                }
                Subscription subscription = client.Subscription.Create(options);
                string subscriptionId = subscription["id"].ToString();
                discountIdViewModel.RazorpaySubscriptionId = subscriptionId;
                discountIdViewModel.CsId = Csid;
                discountIdViewModel.PlanId = OrderDetailViewModel.OrderId;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                    string responseString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseString);
                }
                string? oldSubscriptionId = null;
                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetRazorpaySubscriptionIdByOrderId?orderid={orderId}"; var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(apiResponse))
                        {
                            dynamic result = JsonConvert.DeserializeObject(apiResponse);
                            oldSubscriptionId = result?.subscriptionId;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(oldSubscriptionId))
                {
                    HttpContext.Session.SetString("OldSubscriptionId", oldSubscriptionId);
                }
                else
                {
                    HttpContext.Session.Remove("OldSubscriptionId");
                }

                var vm = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    OrderId = subscription["id"].ToString(),
                    Amount = totalValue,
                    Email = customerEmail,
                    Phone = customerPhone,
                    CustomerName = OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName,
                    isUpgrade = true,
                    IsAddonPayment = false
                };

                return View("RazorpayPaymentPage", vm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public IActionResult SaveOrderSession([FromBody] OrderDetailViewModel model)
        {
            HttpContext.Session.SetString(
                "OrderDetailsJson",
                JsonConvert.SerializeObject(model)
            );

            return Ok();
        }
        public async Task<IActionResult> NormalPaymentInitiate(double discountvalue, double taxpercentage, double totalTaxValue, int packCount, double packCost, string jsonData, int Csid, double totalValue, string orderId, string planId, string customerEmail, string customerPhone, double totalBaseAmount)
        {
            try
            {
                string? sessionData = HttpContext.Session.GetString("OrderDetailsJson");
                RazorpayDiscountIdViewModel discountIdViewModel = new RazorpayDiscountIdViewModel();
                HttpContext.Session.SetString("OrderID", orderId);
                HttpContext.Session.SetString("BaseAmount", totalBaseAmount.ToString());
                OrderDetailViewModel = JsonConvert.DeserializeObject<OrderDetailViewModel>(sessionData) ?? new OrderDetailViewModel();
                OrderDetailViewModel.packCost = packCost;
                OrderDetailViewModel.packCount = packCount;
                string updatedjsonData = JsonConvert.SerializeObject(OrderDetailViewModel);
                HttpContext.Session.SetString("OrderDetailJson", updatedjsonData);
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                int days = Convert.ToInt32(OrderDetailViewModel.SelectedSubscription.NoofDays);
                int sbfid = OrderDetailViewModel.SelectedSubscription.Sbfid;
                double subsAmount = OrderDetailViewModel.SelectedSubscription.Amount + packCost;
                //double taxv = totalValue - totalTaxValue + discountvalue;
                double taxCalc = (subsAmount * OrderDetailViewModel.TaxPercentage) / 100;
                double newPlanAmount = subsAmount + taxCalc;
                if (packCount > 0 && packCost > 0)
                {
                    HttpContext.Session.SetString("newPlanAmount", newPlanAmount.ToString());
                }
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
            { "notes", new Dictionary<string, string> { { "CustomerId", Csid.ToString() } } },
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
                discountIdViewModel.CsId = Csid;
                discountIdViewModel.PlanId = finalPlanId;
                discountIdViewModel.TransactionId = OrderDetailViewModel.OrderId;
                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromHours(1);
                    var content = new StringContent(JsonConvert.SerializeObject(discountIdViewModel), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{Common.GetBaseAddress()}SubscriptionMaster/SaveRazorpaySubscriptionId", content);
                    string responseString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseString);
                }
                string? oldSubscriptionId = null;
                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetRazorpaySubscriptionIdByOrderId?orderid={orderId}"; var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(apiResponse))
                        {
                            dynamic result = JsonConvert.DeserializeObject(apiResponse);
                            oldSubscriptionId = result?.subscriptionId;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(oldSubscriptionId))
                {
                    HttpContext.Session.SetString("OldSubscriptionId", oldSubscriptionId);
                }
                else
                {
                    HttpContext.Session.Remove("OldSubscriptionId");
                }
                var vm = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    OrderId = subscription["id"].ToString(),
                    Amount = totalValue,
                    Email = customerEmail,
                    Phone = customerPhone,
                    CustomerName = OrderDetailViewModel?.PortalCustomerDetails?.ContactPersonName,
                    isUpgrade = true,
                    IsAddonPayment = false
                };
                return View("RazorpayPaymentPage", vm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ServiceUnavailable", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> PaymentSuccess(string razorpay_payment_id, string razorpay_subscription_id, string razorpay_signature)
        {
            try
            {
                double baseAmount = Convert.ToDouble(HttpContext.Session.GetString("BaseAmount") ?? "0");
                double newPlanAmount = Convert.ToDouble(HttpContext.Session.GetString("newPlanAmount") ?? "0");
                string oldOrderID = HttpContext.Session.GetString("OrderID") ?? "";
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                ResponseModel responseModel = new ResponseModel();
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
                    Payment payment = client.Payment.Fetch(razorpay_payment_id);//Payment Fetch
                    string status = payment["status"]?.ToString() ?? "unknown";
                    string txnId = payment["id"]?.ToString() ?? "N/A";
                    // Amount comes in paise
                    decimal amountInPaise = Convert.ToDecimal(payment["amount"] ?? 0);
                    decimal amountInRupees = amountInPaise / 100; // ₹ value
                    paymentResponse.TxnID = txnId;
                    paymentResponse.ResponseCode = (status == "captured") ? "Success" : "Failed";
                    paymentResponse.Amount = (decimal?)baseAmount;
                    paymentResponse.MerchantId = csid.ToString();

                    string membershipid = HttpContext.Session.GetString("membershipid") ?? "";
                    string subscriptionId = HttpContext.Session.GetString("OldSubscriptionId") ?? "";
                    paymentResponse.MerchantTxnNo = membershipid;
                    //OrderDetailViewModel = (String.IsNullOrEmpty(OrderDetailViewModel.RenewalMembershipId)) ? "Subscription" : "Renewal";
                    OrderDetailViewModel.paymentResponse = paymentResponse;
                    OrderDetailViewModel.oldOrderId = oldOrderID;

                    if (status == "captured" && !string.IsNullOrEmpty(subscriptionId))
                    {
                        string cancelMessage = await CancelRazorpaySubscription(subscriptionId);
                        ViewBag.subscriptionCancelMessage = cancelMessage;
                    }

                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/SubscriptionRenewal", stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiresponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                    {
                        InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

                        using (var httpClient = Common.GetHttpClient())
                        {
                            var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Renewal");
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
                        
                        if (newPlanAmount > 0)
                        {
                            OrderDetailViewModel.SelectedSubscription.Amount =Convert.ToInt32( newPlanAmount);
                        }

                        UpgradeEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceNumber}.pdf", amountInRupees, OrderDetailViewModel.SelectedSubscription.Amount);


                        var successModel = new PaymentResponse
                        {
                            Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                            Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            OrderId = OrderDetailViewModel.OrderId,
                            PaymentID = razorpay_payment_id,
                            Status = status,
                            OrderType = "Renewal"
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
        public async Task<IActionResult> PaymentSuccessold(string paymentId, string orderId, string razorpay_signature)
        {
            try
            {
                double baseAmount = Convert.ToDouble(HttpContext.Session.GetString("BaseAmount") ?? "0");
                string oldOrderID = HttpContext.Session.GetString("OrderID") ?? "";
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                ResponseModel responseModel = new ResponseModel();
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
                    // Amount comes in paise
                    decimal amountInPaise = Convert.ToDecimal(payment["amount"] ?? 0);
                    decimal amountInRupees = amountInPaise / 100; // ₹ value
                    paymentResponse.TxnID = txnId;
                    paymentResponse.ResponseCode = (status == "captured") ? "Success" : "Failed";
                    paymentResponse.Amount = (decimal?)baseAmount;
                    paymentResponse.MerchantId = csid.ToString();

                    string membershipid = HttpContext.Session.GetString("membershipid") ?? "";
                    string subscriptionId = HttpContext.Session.GetString("OldSubscriptionId") ?? "";
                    paymentResponse.MerchantTxnNo = membershipid;
                    //OrderDetailViewModel = (String.IsNullOrEmpty(OrderDetailViewModel.RenewalMembershipId)) ? "Subscription" : "Renewal";
                    OrderDetailViewModel.paymentResponse = paymentResponse;
                    OrderDetailViewModel.oldOrderId = oldOrderID;

                    if (status == "captured" && !string.IsNullOrEmpty(subscriptionId))
                    {
                        string cancelMessage = await CancelRazorpaySubscription(subscriptionId);
                        ViewBag.subscriptionCancelMessage = cancelMessage;
                    }

                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/SubscriptionRenewal", stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiresponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                    {
                        InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

                        using (var httpClient = Common.GetHttpClient())
                        {
                            var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + OrderDetailViewModel.OrderId + "/Renewal");
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
                        UpgradeEmailWithInvoice(toEmail, contactPerson, paymentResponse, pdfBytes, $"Invoice_{invoice.InvoiceNumber}.pdf", amountInRupees, OrderDetailViewModel.SelectedSubscription.Amount);


                        var successModel = new PaymentResponse
                        {
                            Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                            Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            OrderId = OrderDetailViewModel.OrderId,
                            PaymentID = paymentId,
                            Status = status,
                            OrderType = "Renewal"
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
                return Content("Error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentFailure(string orderId, string status, string errorDescription, string paymentType)
        {
            try
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                ResponseModel responseModel = new ResponseModel();
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
                paymentResponse.TxnID = "N/A";
                paymentResponse.ResponseCode = finalStatus;

                OrderDetailViewModel.paymentResponse = paymentResponse;

                using (var httpClient = Common.GetHttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(OrderDetailViewModel), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/SubscriptionRenewal", stringContent))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string apiresponse = await response.Content.ReadAsStringAsync();

                            responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                        }
                        else
                        {
                            string error = await response.Content.ReadAsStringAsync();
                        }
                    }
                }


                if (!String.IsNullOrEmpty(OrderDetailViewModel.OrderId))
                {
                    string toEmail = OrderDetailViewModel.PortalCustomerDetails?.EmailAddress ?? String.Empty;
                    string contactPerson = OrderDetailViewModel.PortalCustomerDetails?.ContactPersonName ?? String.Empty;
                    //sendEmailWithInvoice(toEmail, contactPerson, paymentResponse, null, "");
                    var successModel = new PaymentResponse
                    {
                        Name = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Email = orderDetail.PortalCustomerDetails?.EmailAddress,
                        Phone = orderDetail.PortalCustomerDetails?.ContactPersonName,
                        Amount = orderDetail.SelectedSubscription?.Amount ?? 0,
                        OrderId = (paymentType == "Normal" && (finalStatus == "failed" || finalStatus == "cancelled")) ? null : OrderDetailViewModel.OrderId,
                        PaymentID = paymentId,
                        Status = finalStatus
                    };                   
                    return RedirectToAction("RazorPaymentResponse", successModel);
                }              
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
            ViewBag.OrderType = model.OrderType;


            ViewBag.paymentStatus = (model.Status == "captured" || model.Status == "authorized") ? "Success" : "Failed";
            return View("RazorPaymentResponse", model);
        }

        [HttpPost]
        public async Task<IActionResult> PayAddon( double gstPercentage, string ContactPersonName, string orderId, string MembershipId, decimal gstamount, decimal packCost, decimal OriginalAddonAmount, int packCount, int totalUsers, string Phonenumber, string EmailAddress, string Address)
        {
            try
            {


                var addonData = new AddOnPaymentViewModel
                {
                    MembershipId = MembershipId,
                    PackCost = packCost,
                    NumberOfPack = packCount,
                    totalUsers = totalUsers,
                    //added gstamount for net amount--Anshuman 24.12.2025
                    NetAmount = packCost + gstamount,
                    Phone = Phonenumber,
                    Email = EmailAddress,
                    Address = Address,
                    TaxAmount = gstamount,
                    ContactPersonName = ContactPersonName
                };
                HttpContext.Session.SetString("AddonPaymentData", JsonConvert.SerializeObject(addonData));
                string? oldSubscriptionId = await GetSubscriptionIdFromApi(orderId);
                if (string.IsNullOrEmpty(oldSubscriptionId))
                {
                    TempData["error"] = "Subscription ID not found.";
                    return RedirectToAction("MySubscriptionDetails");
                }
                string? key = _config["Razorpay:Key"];
                string? secret = _config["Razorpay:Secret"];             
                RazorpayClient client = new RazorpayClient(key!, secret!);
                int totalAmountInPaise = Convert.ToInt32((packCost + gstamount) * 100);// Here  Addon Amount Pass for next billing cycle
                decimal originalgstAmount= (OriginalAddonAmount*Convert.ToDecimal(gstPercentage))/ 100;
                int totalAmountInPaiseforsubscription = Convert.ToInt32((OriginalAddonAmount+ originalgstAmount) * 100);// Here  Addon Amount Pass for next billing cycle
                var addonUrl = $"https://api.razorpay.com/v1/subscriptions/{oldSubscriptionId}/addons";
                var addonPayload = new
                {
                    item = new
                    {
                        name = $"Addon Pack - {packCount}",
                        amount = totalAmountInPaiseforsubscription,
                        currency = "INR"
                    },
                    quantity = 1
                };

                using (var http = new HttpClient())
                {
                    var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{key}:{secret}"));
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
                    var content = new StringContent(JsonConvert.SerializeObject(addonPayload), Encoding.UTF8, "application/json");
                    var addonResponse = await http.PostAsync(addonUrl, content);
                    if (!addonResponse.IsSuccessStatusCode)
                    {
                        var err = await addonResponse.Content.ReadAsStringAsync();
                        TempData["error"] = $"Failed to create addon: {err}";
                        return RedirectToAction("MySubscriptionDetails");
                    }
                }
                var orderOptions = new Dictionary<string, object>
                {
                    { "amount", totalAmountInPaise },//Here pass the current addon amount for payment
                    { "currency", "INR" },
                    { "receipt", orderId },
                    { "payment_capture", 1 }
                };
                Order order = client.Order.Create(orderOptions);     
                var vm = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    OrderId = order["id"].ToString(),
                    Amount = Convert.ToDouble(packCost + gstamount),
                    Email = EmailAddress,
                    Phone = Phonenumber,
                    CustomerName = ContactPersonName,
                    IsAddonPayment = true
                };
                return View("RazorpayPaymentPage", vm);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
                return RedirectToAction("MySubscriptionDetails");
            }
        }

        private async Task<string?> GetSubscriptionIdFromApi(string orderId)
        {
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    var url = Common.GetBaseAddress() + $"SubscriptionMaster/GetSubscriptionId?orderid={orderId}";
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<dynamic>(apiResponse);
                        var subId = result?.subscriptionId;
                        return subId != null ? subId.ToString() : null;
                    }
                }
            }
            catch
            {
            }
            return null;
        }





        [HttpPost]
        public IActionResult PayAddonOld(string ContactPersonName, string orderId, string MembershipId, decimal gstamount, decimal packCost, int packCount, int totalUsers, string Phonenumber, string EmailAddress, string Address)
        {
            try
            {

                var addonData = new AddOnPaymentViewModel
                {
                    MembershipId = MembershipId,
                    PackCost = packCost,
                    NumberOfPack = packCount,
                    totalUsers = totalUsers,
                    //added gstamount for net amount--Anshuman 24.12.2025
                    NetAmount = packCost + gstamount,
                    Phone = Phonenumber,
                    Email = EmailAddress,
                    Address = Address,
                    TaxAmount = gstamount,
                    ContactPersonName = ContactPersonName
                };
                HttpContext.Session.SetString("AddonPaymentData", JsonConvert.SerializeObject(addonData));
                commonservice = new CommonServices();
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                RazorpayClient client = new RazorpayClient(key, secret);
                var options = new Dictionary<string, object>();
                //options.Add("amount", Convert.ToInt32(packCost * 100));
                //added gstamount for net amount--Anshuman 24.12.2025
                options.Add("amount", Convert.ToInt32((packCost + gstamount) * 100));
                options.Add("currency", "INR");
                options.Add("receipt", OrderDetailViewModel.OrderId);
                options.Add("payment_capture", 1);
                Order order = client.Order.Create(options);
                OrderDetailViewModel.RazorpayOrderId = order["id"].ToString();
                var vm = new RazorpayCheckoutViewModel
                {
                    Key = key,
                    OrderId = order["id"].ToString(),
                    Amount = Convert.ToDouble(packCost),
                    Email = EmailAddress,
                    Phone = Phonenumber,
                    CustomerName = ContactPersonName,
                    IsAddonPayment = true
                };
                return View("RazorpayPaymentPage", vm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> AddonPaymentSuccess(string razorpay_payment_id, string orderId, string razorpay_signature)
        {

            try
            {
                AddOnPaymentViewModel addOnPaymentViewModel = new AddOnPaymentViewModel();
                var addonDataJson = HttpContext.Session.GetString("AddonPaymentData");
                var addonData = !string.IsNullOrEmpty(addonDataJson) ? JsonConvert.DeserializeObject<AddOnPaymentViewModel>(addonDataJson) : new AddOnPaymentViewModel();
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                ResponseModel responseModel = new ResponseModel();
                string key = _config["Razorpay:Key"];
                string secret = _config["Razorpay:Secret"];
                string payload = orderId + "|" + razorpay_payment_id;
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
                    // Amount comes in paise
                    decimal amountInPaise = Convert.ToDecimal(payment["amount"] ?? 0);
                    decimal amountInRupees = amountInPaise / 100;
                    addOnPaymentViewModel.TransactionId = txnId;
                    addOnPaymentViewModel.TransactionStatus = (status == "captured") ? "Success" : "Failed";
                    addOnPaymentViewModel.MembershipId = addonData.MembershipId;
                    string neworderid = commonservice.GenerateOrderNumber();
                    addOnPaymentViewModel.OrderId = neworderid;
                    addOnPaymentViewModel.PackCost = addonData.PackCost;
                    addOnPaymentViewModel.NumberOfPack = addonData.NumberOfPack;
                    addOnPaymentViewModel.Address = addonData.Address;
                    addOnPaymentViewModel.TaxAmount = addonData.TaxAmount;
                    addOnPaymentViewModel.NetAmount = addonData.PackCost + addOnPaymentViewModel.TaxAmount;
                    addOnPaymentViewModel.CreatedBy = csid;
                    paymentResponse.TxnID = txnId;
                    using (var httpClient = Common.GetHttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMinutes(5);
                        StringContent stringContent = new StringContent(JsonConvert.SerializeObject(addOnPaymentViewModel), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SubscriptionMaster/ProcessAddOnPayment", stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiresponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiresponse) ?? new ResponseModel();
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(neworderid))
                    {
                        InvoicePrintDetailsViewModel invoice = new InvoicePrintDetailsViewModel();

                        using (var httpClient = Common.GetHttpClient())
                        {
                            var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GenerateInvoice/" + neworderid + "/Subscription");
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
                        string toEmail = addonData.Email ?? "";
                        string contactPerson = addonData.ContactPersonName ?? "";
                        AddOnPurchaseSuccessEmail(addonData.Email, addonData.ContactPersonName, addOnPaymentViewModel, pdfBytes, $"Addon_Invoice_{invoice.InvoiceId}.pdf");


                        var successModel = new PaymentResponse
                        {

                            Name = addonData.ContactPersonName,
                            Email = addonData.Email,
                            Phone = addonData.Phone,
                            Amount = Convert.ToDecimal(payment["amount"]) / 100,
                            PaymentID = razorpay_payment_id,
                            OrderId = neworderid,
                            Status = status,
                            OrderType = "Addon"


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

        public void AddOnPurchaseSuccessEmail(string toEmail, string contactPerson, AddOnPaymentViewModel addOnPaymentViewModel, byte[] attachmentData, string attachmentFileName)
        {
            string userKey = APIEncryptionHelper.Encrypt(toEmail);
            string loginUrl = Common.GetLandingPageAddress();

            string messageBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Add-On Purchase Successful</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background:#f9f9f9; margin:0; padding:0; }}
        .container {{ max-width:600px; margin:20px auto; background:#fff; border-radius:12px; overflow:hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }}
        .header {{ background:#0d6efd; color:#fff; padding:25px; text-align:center; }}
        .header h2 {{ margin:0; font-weight: 300; font-size: 24px; }}
        .content {{ padding:30px; color:#444; line-height: 1.6; }}
        .box {{ background:#f1f5ff; padding:20px; border-radius: 8px; border-left:5px solid #0d6efd; margin:20px 0; }}
        .box p {{ margin: 5px 0; font-size: 15px; }}
        .footer {{ background:#f4f4f4; padding:20px; text-align:center; font-size:13px; color:#777; border-top: 1px solid #eee; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Add-On Purchase Successful</h2>
        </div>
        <div class='content'>
            <p>Dear {contactPerson},</p>
            <p>Your add-on purchase has been completed successfully. The payment has been received and your new feature is now active.</p>

            <div class='box'>
                <p><strong>Transaction ID:</strong> {addOnPaymentViewModel.TransactionId}</p>
                <p><strong>Order ID:</strong> {addOnPaymentViewModel.OrderId}</p>           
                <p><strong>Net Amount Paid:</strong> {addOnPaymentViewModel.NetAmount:C}</p>           
            </div>

            <p><strong>Important Note:</strong> Please note that your next billing cycle will automatically include this add-on amount, and your total subscription fee will be adjusted accordingly.</p>
            
            <p>Your official invoice is attached with this email for your records. If you need any assistance, feel free to reply to this email.</p>
            <p>Best regards,<br/><strong>thecloudTree.ai | HCM</strong></p>
        </div>


        <div class='footer'>
            <p>info@thecloudtree.ai | (+91) 9090973033</p>
        </div>
    </div>
</body>
</html>";

            EmailHelper.sendEmail(toEmail, messageBody, "Add-On Purchase Successful", attachmentData, attachmentFileName);
        }

    }
}
