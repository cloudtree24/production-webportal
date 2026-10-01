using HCMPortal.Models;
using HCMPortal.Service;
using System.Text;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HCMPortal.Helper;
using Microsoft.AspNetCore.Authentication;
using System.Reflection;

namespace HCMPortal.Controllers
{
    public class SupportManagementController : Controller
    {
        private CommonServices commonService = new CommonServices();
        private readonly IWebHostEnvironment hostingenvironment;
        private readonly ZohoAuthService zohoAuthService;
        public SupportManagementController(IWebHostEnvironment _hostingenvironment, ZohoAuthService zohoAuthService)
        {
            hostingenvironment = _hostingenvironment;
            this.zohoAuthService = zohoAuthService;
        }


        // GET: SupportManagementController
        public async Task<ActionResult> SupportDetails(SupportPageViewModel model)
        {
            List<TicketMaster> tickets = new List<TicketMaster>();
        SupportPageViewModel supportPageViewModel = new SupportPageViewModel();
            
            using (var httpClient = Common.GetHttpClient())
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SupportManagement/GetAllTicketDetailsByCustomerId/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tickets = JsonConvert.DeserializeObject<List<TicketMaster>>(apiResponse) ?? new List<TicketMaster>();
                    }
                }
            }
            supportPageViewModel.ticketList= tickets;
            //supportPageViewModel.Tmid = (TicketId != 0) ? 1 : 0;
            //supportPageViewModel.Tmid = ticketMaster.Tmid;
            //supportPageViewModel.TicketId = ticketMaster.TicketId;
            //supportPageViewModel.Subject = ticketMaster.Subject;
            //supportPageViewModel.ResolveStatus = ticketMaster.ResolveStatus;
            //supportPageViewModel.CreatedDate = ticketMaster.CreatedDate;
            //supportPageViewModel.ClosedDate = ticketMaster.ClosedDate;
            ViewBag.Success = TempData["Success"];
            ViewBag.Exception = TempData["Exception"];
            return View(supportPageViewModel);
        }
        [HttpPost]
        public async Task<ActionResult> TicketGenerate(SupportPageViewModel supportPageViewModel)
        {
            ResponseModel responseModel = new ResponseModel();
            if (supportPageViewModel.attachment != null && supportPageViewModel.attachment.Length > 0)
            {
                //supportPageViewModel.AttachedFileName = commonService.UploadFile(supportPageViewModel.attachment, hostingenvironment);
                supportPageViewModel.AttachedFileName =await commonService.UploadFile(supportPageViewModel.attachment, hostingenvironment,true);
            }
            supportPageViewModel.CreatedBy = HttpContext.Session.GetInt32("UserId").ToString();

            supportPageViewModel.CreatedDate = DateTime.Now;
            supportPageViewModel.ResolveStatus = (supportPageViewModel.ResolveStatus != null) ? supportPageViewModel.ResolveStatus : "Open";
            supportPageViewModel.CustomerId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

            int lastRecordId = await commonService.GetLastTicketid();
            supportPageViewModel.TicketId = (lastRecordId == 0) ? new Random().Next(1000, 1000000000) : lastRecordId + 1;

            (string ZTicketId, string ZAttachMentid) = await PostTicketToZoho(supportPageViewModel);



            supportPageViewModel.ZohoTicketId = ZTicketId;
            supportPageViewModel.ZohoAttachMentId = ZAttachMentid;
            using (var httpClient = Common.GetHttpClient())
            {
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(supportPageViewModel), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync(Common.GetBaseAddress() + "SupportManagement/SaveAndUpdateTicket", stringContent))
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
                TempData["Success"] = responseModel.Message;
            }
            else
            {
                TempData["Exception"] = responseModel.Message;
            }
            return RedirectToAction("SupportDetails");
        }

        //private async Task<string> PostTicketToZoho(SupportPageViewModel model)
        private async Task<(string TicketId, string AttachmentId)> PostTicketToZoho(SupportPageViewModel model)
        {

            try
            {
                // 1. Check if we are totally new (No Refresh Token yet)
                if (string.IsNullOrEmpty(ZohoTokenStore.RefreshToken))
                {
                    // This uses the "Generated_code" from appsettings.json
                    // It will populate BOTH AccessToken AND RefreshToken in the store

                    if (!ZohoTokenStore.IsExpired)
                        await zohoAuthService.GetInitialTokenAsync();
                    else
                       await zohoAuthService.RefreshAccessTokenAsync(ZohoTokenStore.RefreshToken);
                }
                // 2. If we have a Refresh Token but the Access Token is expired/null
                else if (string.IsNullOrEmpty(ZohoTokenStore.AccessToken) || ZohoTokenStore.IsExpired)
                {
                    await zohoAuthService.RefreshAccessTokenAsync(ZohoTokenStore.RefreshToken);
                }
                using (var zohoClient = new HttpClient())
                {
                    // Set the endpoint for India region
                    //var url = "https://desk.zoho.in/api/v1/tickets";

                    // Add Headers exactly like the curl example
                    zohoClient.DefaultRequestHeaders.Clear();
                    //zohoClient.DefaultRequestHeaders.Add("orgId", "60061270292"); // Your OrgId--thecloudtree.ai
                    zohoClient.DefaultRequestHeaders.Add("orgId", ZohoConfig.DESKOrgId); // Your OrgId--anantaautomobiles

                    // Note: Zoho Desk accepts "Zoho-oauthtoken" OR "Bearer". 
                    // Using "Zoho-oauthtoken" to match your documentation example exactly:
                    zohoClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken " + ZohoTokenStore.AccessToken);
                    PortalCredentialViewModel customerDetails = await commonService.GetPortalCustomerdetails(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")));
                    // 2. Map your model to the Zoho JSON structure
                    var zohoPayload = new
                    {
                        subject = model.Subject,
                        description = model.Summary,
                        //departmentId = "241001000000010772", // Your specific Department ID--thecloudtree.ai
                        departmentId = ZohoConfig.DESKDepartmentId, // Your specific Department ID--anantaautomobiles
                        contact = new
                        {
                            //lastName = "Customer", // Required if contactId is not used
                            lastName = customerDetails.ContactPersonName, // Required if contactId is not used
                            email = HttpContext.Session.GetString("Emailaddress") ?? string.Empty,
                            phone= customerDetails.Phonenumber+""
                        },
                        category = model.QueryType,
                        priority = "High",
                        status = "Open",
                        channel = "Web" // Setting this to Web since it's from your portal
                    };

                    string zohoJson = JsonConvert.SerializeObject(zohoPayload);
                    var zohoContent = new StringContent(zohoJson, Encoding.UTF8, "application/json");

                    // 3. Execute the POST
                    var response = await zohoClient.PostAsync(ZohoConfig.DESKBaseUrl, zohoContent);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse as dynamic to easily access properties without a class
                        dynamic result = JsonConvert.DeserializeObject(responseBody);

                        // You can now access Zoho's data:
                        string zohoTicketId = result.id;           // The long numerical ID
                        string zohoTicketNumber = result.ticketNumber; // The display ID like #101
                        string attachmentId = await UploadAttachmentToZoho(zohoTicketId, model.attachment);
                        // You can log this or save it to your local DB here
                        Console.WriteLine($"Successfully created Zoho Ticket: {zohoTicketNumber}");

                        //return true;
                        return (zohoTicketId,attachmentId);
                    }
                    else
                    {
                        // Capture error details for debugging
                        var errorBody = await response.Content.ReadAsStringAsync();
                        // You can set a breakpoint here to see why it failed
                        return (string.Empty,string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception
                return (string.Empty, string.Empty);
            }
        }
        private async Task<string> UploadAttachmentToZoho(string ticketId, IFormFile attachment)
        {
            try
            {
                using (var zohoClient = new HttpClient())
                {
                    zohoClient.BaseAddress = new Uri("https://desk.zoho.in/");
                    zohoClient.DefaultRequestHeaders.Add("Authorization", "Zoho-oauthtoken " + ZohoTokenStore.AccessToken);
                    //zohoClient.DefaultRequestHeaders.Add("orgId", "60061270292"); // Your OrgId--thecloudtree.ai
                    zohoClient.DefaultRequestHeaders.Add("orgId", ZohoConfig.DESKOrgId); // Your OrgId--anantaautomobiles

                    // Prepare the multipart form data
                    using (var content = new MultipartFormDataContent())
                    {
                        // Convert IFormFile to a Stream
                        var fileStream = attachment.OpenReadStream();
                        var fileContent = new StreamContent(fileStream);

                        // Add the file to the content - "file" is the parameter name Zoho expects
                        content.Add(fileContent, "file", attachment.FileName);

                        var response = await zohoClient.PostAsync($"api/v1/tickets/{ticketId}/attachments", content);

                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            // Parse as dynamic to easily access properties without a class
                            dynamic result = JsonConvert.DeserializeObject(responseBody);
                            // You can now access Zoho's data:
                            string zohoAttachmentId = result.id;           // The long numerical ID
                            return zohoAttachmentId;
                        }
                        else
                        {
                            // Capture error details for debugging
                            var errorBody = await response.Content.ReadAsStringAsync();
                            // You can set a breakpoint here to see why it failed
                            return string.Empty;
                        }
                    }
                }
            }

            catch (Exception)
            {
                return string.Empty;
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetLastTicketDetailsById(int ticketId)
        {
            TicketMaster ticketMaster = new TicketMaster();
            using (var httpClient = Common.GetHttpClient())
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SupportManagement/GetTicketDetailsById/" + ticketId))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        ticketMaster = JsonConvert.DeserializeObject<TicketMaster>(apiResponse) ?? new TicketMaster();
                    }
                }
            }
            return Json(ticketMaster);
        }

        public async Task<ActionResult> TicketFilterResults(SupportPageViewModel model)
        {
            List<TicketMaster> tickets = new List<TicketMaster>();
            SupportPageViewModel supportPageViewModel = new SupportPageViewModel();

            using (var httpClient = Common.GetHttpClient())
            {
                int csid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "SupportManagement/GetAllTicketDetailsByCustomerId/" + csid))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tickets = JsonConvert.DeserializeObject<List<TicketMaster>>(apiResponse) ?? new List<TicketMaster>();
                    }
                }
            }
            model.FromDate = (model.FromDate.HasValue) ? model.FromDate.Value.Date :DateTime.Now.Date;
            if(model.Category!=null)
            {
                tickets = tickets.Where(e => e.CreatedDate.Date >= model.FromDate && e.CreatedDate.Date <= model.ToDate.Date && e.QueryType == model.Category).ToList();
            }
            else
            {
                tickets = tickets.Where(e => e.CreatedDate.Date >= model.FromDate && e.CreatedDate.Date <= model.ToDate.Date).ToList();
            }
            supportPageViewModel.ticketList = tickets;
            //ViewBag.Success = TempData["Success"];
            //ViewBag.Exception = TempData["Exception"];
            return View("SupportDetails", supportPageViewModel);
        }

    }
}
