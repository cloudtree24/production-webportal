using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.Service;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace HCMPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private CommonServices? commonService;
        private readonly IWebHostEnvironment hostingenvironment;
        private readonly SaveAmsNotification _notificationService;
        private static List<BlogViewModel> AllActiveblogList=new List<BlogViewModel>();
        public HomeController(ILogger<HomeController> logger, SaveAmsNotification notificationService, IWebHostEnvironment _hostingenvironment)
        {
            _logger = logger;
            _notificationService = notificationService;
            hostingenvironment = _hostingenvironment;

        }

        //[HttpGet]
        [HttpGet("customers/blogs/{slug}", Name = "BlogDetails")]
        public async Task<IActionResult> Blogdetails(string slug)
        {
            DynamicPage page = new DynamicPage();
            DynamicPageViewModel pageviewModel = new DynamicPageViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DynamicPage/GetPageBySlug/" + slug))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        pageviewModel = JsonConvert.DeserializeObject<DynamicPageViewModel>(apiResponse) ?? new DynamicPageViewModel();
                        //page.OldSlug = page.Slug;
                        //page.IsReadyToPublish = (page.IsPublish == 1) ? true : false;
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            //if page is for blog Detials 
            if (pageviewModel.PageType == "Blog")
            {
                //return RedirectToAction("Blogdetails", "Home", pageviewModel);
                //return RedirectToAction("Blogdetails", "Home", new { slug = pageviewModel.Slug });
                List<BlogViewModel> blogList = new List<BlogViewModel>();
                CommonServices commonServices = new CommonServices();
                List<BlogViewModel> related = new List<BlogViewModel>();
                related = await commonServices.GetAllActiveBlogList();
                //DynamicPageViewModel pageViewModel = related.Where(e => e.WebSlugName == slug).Select(e => new DynamicPageViewModel()
                //{
                //    Title = e.PostTitle,
                //    Slug = e.WebSlugName,
                //    MetaTitle = e.PostTitle,
                //    HtmlContent = e.Description,
                //    BannerImagePath = e.ImagePath,
                //    Category = e.Category,
                //    PageType = "Blog",
                //    CreatedDate = e.CreatedDate,
                //    ViewsCount = e.ViewsCount
                //}).FirstOrDefault() ?? new DynamicPageViewModel();

                if (!String.IsNullOrEmpty(pageviewModel.HtmlContent))
                {
                    blogList = (related).Where(e => e.WebSlugName != pageviewModel.Slug && e.Category == pageviewModel.Category).ToList();
                    if (blogList.Count() == 0)
                    {
                        blogList = related;
                    }
                    pageviewModel.relatedBlogs = blogList.Where(e => e.WebSlugName != pageviewModel.Slug).Take(5).ToList();
                }
                else
                {
                    pageviewModel.HtmlContent = "<p>No Content Found</p>";
                }
            }
            return View(pageviewModel);
        }

        [HttpGet("hcm-solution/{slug}", Name = "PageDetails",Order =1)]
        [HttpGet("customers/{slug}", Name = "CustomerPageDetails", Order = 1)]
        [HttpGet("company/{slug}", Name = "CompanyPageDetails", Order = 1)]
        [HttpGet("{slug}", Name = "ComplianceDetails", Order = 1)]
        public async Task<IActionResult> Pagedetails(string slug)
        {
            DynamicPage page = new DynamicPage();
            DynamicPageViewModel pageviewModel = new DynamicPageViewModel();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DynamicPage/GetPageBySlug/" + slug))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        pageviewModel = JsonConvert.DeserializeObject<DynamicPageViewModel>(apiResponse) ?? new DynamicPageViewModel();
                        //page.OldSlug = page.Slug;
                        //page.IsReadyToPublish = (page.IsPublish == 1) ? true : false;
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            //if page is for blog Detials 
            if (pageviewModel.PageType == "Blog")
            {
                //return RedirectToAction("Blogdetails", "Home", pageviewModel);
                //return RedirectToAction("Blogdetails", "Home", new { slug = pageviewModel.Slug });
                List<BlogViewModel> blogList = new List<BlogViewModel>();
                CommonServices commonServices = new CommonServices();
                List<BlogViewModel> related = new List<BlogViewModel>();
                related = await commonServices.GetAllActiveBlogList();
                if (!String.IsNullOrEmpty(pageviewModel.HtmlContent))
                {
                    blogList = (related).Where(e => e.WebSlugName != pageviewModel.Slug && e.Category == pageviewModel.Category).ToList();
                    if (blogList.Count() == 0)
                    {
                        blogList = related;
                    }
                    pageviewModel.relatedBlogs = blogList.Where(e => e.WebSlugName != pageviewModel.Slug).Take(5).ToList();
                }
                else
                {
                    pageviewModel.HtmlContent = "<p>No Content Found</p>";
                }
            }
            return View("Blogdetails", pageviewModel);
        }

        [HttpGet("customers/blogs", Name = "Blogs")]
        public async Task<IActionResult> Blogs(int page = 1)
        {
            int pageSize = 25;

            CommonServices commonServices = new CommonServices();
            BlogViewModel blogs = new BlogViewModel();

            blogs = await commonServices.GetPagedBlogs(page, pageSize);

            var model = new BlogViewModel
            {
                Blogs = blogs.Blogs,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = blogs.TotalItems,
            };


            return View(model);
        }

        public IActionResult ServiceUnavailable()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {

            SubscriptionPageViewModel suscriptionPageViewModel = new SubscriptionPageViewModel();
            List<WebBanner> bannerList = new List<WebBanner>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "PortalDashBoard/GetAllActiveBanner"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        bannerList = JsonConvert.DeserializeObject<List<WebBanner>>(apiResponse) ?? new List<WebBanner>();
                    }
                }
            }
            foreach (var item in bannerList)
            {
                if (String.IsNullOrEmpty(item.ImageName))
                {
                    item.ImageName = "/assets/images/user_icon.png";
                }
                else
                {
                    // 1. Define the relative path within the wwwroot folder
                    string relativePath = Path.Combine("assets", "UploadImg", item.ImageName);

                    // 2. Get the full physical path on the server
                    // The WebRootPath points to the application's wwwroot folder.
                    string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);

                    // 3. Check if the file exists
                    bool fileExists = System.IO.File.Exists(absolutePath);

                    if (fileExists)
                    {
                        // The file is present
                        item.ImageName = $"/{relativePath}";
                    }
                    else
                    {
                        // The file is not present
                        item.ImageName = Common.ImageAndDocSavePageAddress() + "assets/UploadImg/" + item.ImageName;
                    }
                }
            }
            //// ====================== TESTIMONIAL LIST DAITARI(25-11-2025) ======================
            //List<TestimonialModel> testimonialList = new List<TestimonialModel>();
            //using (var httpClient = Common.GetHttpClient())
            //{
            //    using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Testimonial/GetAllTestimonialList"))
            //    {
            //        if (response.IsSuccessStatusCode)
            //        {
            //            string apiResponse = await response.Content.ReadAsStringAsync();
            //            testimonialList = JsonConvert.DeserializeObject<List<TestimonialModel>>(apiResponse) ?? new List<TestimonialModel>();
            //        }
            //    }
            //}
            //foreach (var item in testimonialList)
            //{
            //    if (String.IsNullOrEmpty(item.pimage))
            //    {
            //        item.pimage = Common.ImageAndDocSavePageAddress().Replace("/api/", "/") + "assets/UploadImg/27102025153016.png";
            //    }
            //    else
            //    {
            //        string relativePath = Path.Combine("assets", "UploadImg", item.pimage);
            //        string absolutePath = Path.Combine(hostingenvironment.WebRootPath, relativePath);
            //        bool fileExists = System.IO.File.Exists(absolutePath);
            //        if (fileExists)
            //        {
            //            item.pimage = $"/{relativePath}";
            //        }
            //        else
            //        {
            //            item.pimage = Common.ImageAndDocSavePageAddress().Replace("/api/", "/") + "assets/UploadImg/" + item.pimage;
            //        }
            //    }
            //}
            //suscriptionPageViewModel.testimonialList = testimonialList;
            suscriptionPageViewModel.bannerList = bannerList;

            List<TblHomeFeatureMaster> featureList = new List<TblHomeFeatureMaster>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "HomeFeature/GetAllFeatures"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        featureList = JsonConvert.DeserializeObject<List<TblHomeFeatureMaster>>(apiResponse) ?? new List<TblHomeFeatureMaster>();
                    }
                }
            }
            suscriptionPageViewModel.featureList = featureList;
            //ViewBag.Exception = TempData["Exception"];
            ViewBag.Success = TempData["Success"];
            TempData["Success"] = null;
            commonService=new CommonServices();
            //set SEO content in the session
            SeoConfigViewModel seoConfig = new SeoConfigViewModel();
            seoConfig = await commonService.GetActiveSEOContent();
            GlobalProperty.MetaDescription=seoConfig.MetaDescription;
            GlobalProperty.MetaTitle=seoConfig.MetaTitle;
            GlobalProperty.GoogleAnalyticsId=seoConfig.GoogleAnalyticsId??String.Empty;
            //HttpContext.Session.SetString("MetaTitle", seoConfig.MetaTitle ?? String.Empty);
            //HttpContext.Session.SetString("MetaDescription", seoConfig.MetaDescription ?? String.Empty);
            //HttpContext.Session.SetString("GoogleAnalyticsId", seoConfig.GoogleAnalyticsId ?? String.Empty);


            return View(suscriptionPageViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //[HttpGet]
        //public  async Task<IActionResult> Blogdetails(string slug)
        //{
        //    List<BlogViewModel> blogList = new List<BlogViewModel>();
        //    CommonServices commonServices = new CommonServices();
        //    List<BlogViewModel> related = new List<BlogViewModel>();
        //    related = await commonServices.GetAllActiveBlogList();
        //    DynamicPageViewModel pageViewModel = related.Where(e => e.WebSlugName == slug).Select(e=>new DynamicPageViewModel()
        //    {
        //        Title = e.PostTitle,
        //        Slug = e.WebSlugName,
        //        MetaTitle = e.PostTitle,
        //        HtmlContent = e.Description,
        //        BannerImagePath = e.ImagePath,
        //        Category = e.Category,
        //        PageType = "Blog",
        //        CreatedDate = e.CreatedDate,
        //        ViewsCount=e.ViewsCount
        //    }).FirstOrDefault()??new DynamicPageViewModel();

        //    if (!String.IsNullOrEmpty(pageViewModel.HtmlContent))
        //        {
        //        blogList = (related).Where(e => e.WebSlugName != pageViewModel.Slug && e.Category == pageViewModel.Category).ToList();
        //        if (blogList.Count() == 0)
        //        {
        //            blogList = related;
        //        }
        //        pageViewModel.relatedBlogs = blogList;
        //    }
        //    else
        //    {
        //        pageViewModel.HtmlContent = "<p>No Content Found</p>";
        //    }
        //    return View(pageViewModel);
        //}
        [HttpGet("company/career", Name = "Career")]
        public async Task< IActionResult> Career()
        {
            CareerViewModel careerViewModel = new CareerViewModel();
            List<TblJobPostDetail> tblJobPostDetails = new List<TblJobPostDetail>();
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() +
                               "Recruitment/GetAllJobPostListForHCMPortal");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    tblJobPostDetails = JsonConvert.DeserializeObject<List<TblJobPostDetail>>(apiResponse)
                                        ?? new List<TblJobPostDetail>();
                }
            }
            careerViewModel.JobList=tblJobPostDetails;
            return View(careerViewModel);
        }

     [HttpPost]
     public async Task<IActionResult> SubmitJobApplication(
     CareerViewModel careerViewModel,
     [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            commonService = new CommonServices();
            ResponseModel responseModel = new ResponseModel();
            careerViewModel.CandidateApplication.CreatedBy=careerViewModel.CandidateApplication.Name;
            if (!await VerifyRecaptcha(gRecaptchaResponse))
            {
                ModelState.AddModelError("", "reCAPTCHA validation failed.");
                ViewBag.OpenModal = true;

                // Reload Job List
                careerViewModel.JobList = await GetAllJobList();

                return View("Career", careerViewModel);
            }
            try
            {
                
                ModelState.Remove("CandidateApplication.CreatedBy");
                if (ModelState.IsValid)
                {
                    if (careerViewModel.CandidateApplication.ResumeFile!=null && careerViewModel.CandidateApplication.ResumeFile.Length>0)
                    {
                        careerViewModel.CandidateApplication.Cvupload=await commonService.UploadFile(careerViewModel.CandidateApplication.ResumeFile, hostingenvironment, true);
                    }
                    using (var httpClient = Common.GetHttpClient())
                    {
                        StringContent stringContent = new StringContent(
                            JsonConvert.SerializeObject(careerViewModel.CandidateApplication),
                            Encoding.UTF8,
                            "application/json");

                        using (var response = await httpClient.PostAsync(
                            Common.GetBaseAddress() + "Recruitment/CreateUpdateCandidateApplication",
                            stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse)
                                                ?? new ResponseModel();
                            }
                            else
                            {
                               string s=await response.Content.ReadAsStringAsync();
                            }
                        }
                    }

                    if (responseModel.IsSuccess)
                    {
                        TempData["ResultType"] = "Success";
                        TempData["ResultMessage"] = "Your Application Submitted Successfully, Our Team will get back to you soon";
                        string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Application Submitted</title>\r\n" +
"<style>\r\n" +
"body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f9f9f9; margin: 0; padding: 0; }\r\n" +
".email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }\r\n" +
".email-header { background: #28a745; color: #ffffff; padding: 20px; text-align: center; }\r\n" +
".email-header h1 { margin: 0; font-size: 22px; }\r\n" +
".email-body { padding: 20px; }\r\n" +
".email-body p { margin: 0 0 15px; }\r\n" +
".email-footer { background: #f4f4f4; text-align: center; padding: 15px; font-size: 14px; color: #777; }\r\n" +
"</style>\r\n" +
"</head>\r\n" +
"<body>\r\n" +
"<div class=\"email-container\">\r\n" +

"    <div class=\"email-header\">\r\n" +
"        <h1>Application Submitted Successfully </h1>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-body\">\r\n" +
"        <p>Dear " + careerViewModel.CandidateApplication.Name + ",</p>\r\n" +

"        <p>Thank you for applying to <strong>" + careerViewModel.SelectedJobTitle + "</strong>.</p>\r\n" +

"        <p>Your job application has been successfully submitted. Our recruitment team will review your profile and get back to you soon.</p>\r\n" +

"        <p>We appreciate your interest in joining our organization and wish you the very best.</p>\r\n" +

"        <p>Thank you,<br><strong>TheCloudTree Recruitment Team</strong></p>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-footer\">\r\n" +
"        <p>thecloudtree.ai</p>\r\n" +
"        <p>Email: info@thecloudtree.ai | Phone: (+91) 9090973033</p>\r\n" +
"    </div>\r\n" +

"</div>\r\n" +
"</body>\r\n" +
"</html>";
                        if (careerViewModel.CandidateApplication.EmailAddress != null)
                        {
                            SendEmail(careerViewModel.CandidateApplication.EmailAddress, "Thank You! Applying for the Post", body);
                        }
                        SelfMasterDetailsViewModel selfMasterDetailsViewModel = new SelfMasterDetailsViewModel();
                        selfMasterDetailsViewModel=await SelfDetailsMaster();

                        string Abody = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>New Job Application</title>\r\n" +
"<style>\r\n" +
"body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f9f9f9; margin: 0; padding: 0; }\r\n" +
".email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; }\r\n" +
".email-header { background: #007bff; color: #ffffff; padding: 20px; text-align: center; }\r\n" +
".email-body { padding: 20px; }\r\n" +
".email-footer { background: #f4f4f4; text-align: center; padding: 15px; font-size: 14px; color: #777; }\r\n" +
"</style>\r\n" +
"</head>\r\n" +
"<body>\r\n" +

"<div class=\"email-container\">\r\n" +

"    <div class=\"email-header\">\r\n" +
"        <h2>New Candidate Application Received.</h2>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-body\">\r\n" +

"        <p>Hello <strong>" + selfMasterDetailsViewModel.ContactPerson+ "</strong>,</p>\r\n" +

"        <p><strong>" + careerViewModel.CandidateApplication.EmailAddress+ "</strong> has applied for the position of <strong>" +
(careerViewModel.SelectedJobTitle ?? "N/A") + "</strong>.</p>\r\n" +

"        <p>You can review the candidate details </p>\r\n" +

"        <p>Applied on: " + DateTime.Now.ToString("dd MMM yyyy hh:mm tt") + "</p>\r\n" +

"        <br/>\r\n" +

"        <p>Regards,<br><strong>TheCloudTree System</strong></p>\r\n" +

"    </div>\r\n" +

"    <div class=\"email-footer\">\r\n" +
"        <p>thecloudtree.ai</p>\r\n" +
"        <p>Email: info@thecloudtree.ai | Phone: (+91) 9090973033</p>\r\n" +
"    </div>\r\n" +

"</div>\r\n" +
"</body>\r\n" +
"</html>";
                        
                        if (selfMasterDetailsViewModel.PrimaryEmail!=null)
                        {
                            SendEmail(selfMasterDetailsViewModel.PrimaryEmail, "New Job Application Received", Abody);
                        }

                        return RedirectToAction("Career");
                    }
                    else
                    {
                        TempData["ResultType"] = "Error";
                        TempData["ResultMessage"] = "Something went wrong! Please try again.";

                        ViewBag.OpenModal = true;
                        careerViewModel.JobList = await GetAllJobList();

                        return View("Career", careerViewModel);
                    }
                }
                else
                {
                    TempData["ResultType"] = "Error";
                    TempData["ResultMessage"] = "Enter all required data";

                    ViewBag.OpenModal = true;
                    careerViewModel.JobList = await GetAllJobList();

                    return View("Career", careerViewModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "Error";
                TempData["ResultMessage"] = "Server error: " + ex.Message;

                ViewBag.OpenModal = true;
                careerViewModel.JobList = await GetAllJobList();

                return View("Career", careerViewModel);
            }
        }

        private async Task<List<TblJobPostDetail>> GetAllJobList()
        {
            List<TblJobPostDetail> jobList = new List<TblJobPostDetail>();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(
                    Common.GetBaseAddress() + "Recruitment/GetAllJobPostListForHCMPortal"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        jobList = JsonConvert.DeserializeObject<List<TblJobPostDetail>>(apiResponse)
                                  ?? new List<TblJobPostDetail>();
                    }
                }
            }

            return jobList;
        }

        [HttpGet("company/jobdetails/{Jid}", Name = "JobDetails")]
        // for URL optimize--Anshuman 13.07.2026
        public async Task<IActionResult> JobDetails(int Jid)
        {
            JobDetailsViewModel jobDetailsViewModel = new JobDetailsViewModel();
            TblJobPostDetail tblJobPostDetail = new TblJobPostDetail();
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() +
                               "Recruitment/GetJobPostDetailsByJid/"+Jid);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    tblJobPostDetail = JsonConvert.DeserializeObject<TblJobPostDetail>(apiResponse)
                                        ?? new TblJobPostDetail();
                }
            }
            jobDetailsViewModel.tblJobPostDetail=tblJobPostDetail;
            return View(jobDetailsViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> JobDetails(JobDetailsViewModel jobDetailsViewModel,
     [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {
            commonService = new CommonServices();
            ResponseModel responseModel = new ResponseModel();
            jobDetailsViewModel.CandidateApplication.CreatedBy=jobDetailsViewModel.CandidateApplication.Name;
            try
            {
                if (!await VerifyRecaptcha(gRecaptchaResponse))
                {
                    ModelState.AddModelError("", "reCAPTCHA validation failed.");
                    ViewBag.OpenModal = true;

                    // Reload Job List
                    // careerViewModel.JobList = await GetAllJobList();

                    jobDetailsViewModel.tblJobPostDetail = await GetJobById(jobDetailsViewModel.CandidateApplication.Jid);
                    return View("JobDetails", jobDetailsViewModel);
                }
                ModelState.Remove("CandidateApplication.CreatedBy");
                if (ModelState.IsValid)
                {
                    if (jobDetailsViewModel.CandidateApplication.ResumeFile!=null && jobDetailsViewModel.CandidateApplication.ResumeFile.Length>0)
                    {
                        jobDetailsViewModel.CandidateApplication.Cvupload=await commonService.UploadFile(jobDetailsViewModel.CandidateApplication.ResumeFile, hostingenvironment, true);
                    }
                    using (var httpClient = Common.GetHttpClient())
                    {
                        StringContent stringContent = new StringContent(
                            JsonConvert.SerializeObject(jobDetailsViewModel.CandidateApplication),
                            Encoding.UTF8,
                            "application/json");

                        using (var response = await httpClient.PostAsync(
                            Common.GetBaseAddress() + "Recruitment/CreateUpdateCandidateApplication",
                            stringContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();

                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse)
                                                ?? new ResponseModel();
                            }
                            else
                            {
                                string s = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }

                    if (responseModel.IsSuccess)
                    {
                        TempData["ResultType"] = "Success";
                        TempData["ResultMessage"] = "Your Application Submitted Successfully, Our Team will get back to you soon";
                        string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Application Submitted</title>\r\n" +
"<style>\r\n" +
"body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f9f9f9; margin: 0; padding: 0; }\r\n" +
".email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }\r\n" +
".email-header { background: #28a745; color: #ffffff; padding: 20px; text-align: center; }\r\n" +
".email-header h1 { margin: 0; font-size: 22px; }\r\n" +
".email-body { padding: 20px; }\r\n" +
".email-body p { margin: 0 0 15px; }\r\n" +
".email-footer { background: #f4f4f4; text-align: center; padding: 15px; font-size: 14px; color: #777; }\r\n" +
"</style>\r\n" +
"</head>\r\n" +
"<body>\r\n" +
"<div class=\"email-container\">\r\n" +

"    <div class=\"email-header\">\r\n" +
"        <h1>Application Submitted Successfully</h1>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-body\">\r\n" +
"        <p>Dear " + jobDetailsViewModel.CandidateApplication.Name + ",</p>\r\n" +

"        <p>Thank you for applying to <strong>" +jobDetailsViewModel.SelectedJobTitle+ "</strong>.</p>\r\n" +

"        <p>Your job application has been successfully submitted. Our recruitment team will review your profile and get back to you soon.</p>\r\n" +

"        <p>We appreciate your interest in joining our organization and wish you the very best.</p>\r\n" +

"        <p>Thank you,<br><strong>TheCloudTree Recruitment Team</strong></p>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-footer\">\r\n" +
"        <p>thecloudtree.ai</p>\r\n" +
"        <p>Email: info@thecloudtree.ai | Phone: (+91) 9090973033</p>\r\n" +
"    </div>\r\n" +

"</div>\r\n" +
"</body>\r\n" +
"</html>";



                        if (jobDetailsViewModel.CandidateApplication.EmailAddress != null)
                        {
                            SendEmail(jobDetailsViewModel.CandidateApplication.EmailAddress, "Thank You! Applying for the Post", body);
                        }
                        SelfMasterDetailsViewModel selfMasterDetailsViewModel = new SelfMasterDetailsViewModel();
                        selfMasterDetailsViewModel=await SelfDetailsMaster();

                        string Abody = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>New Job Application</title>\r\n" +
"<style>\r\n" +
"body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f9f9f9; margin: 0; padding: 0; }\r\n" +
".email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; }\r\n" +
".email-header { background: #007bff; color: #ffffff; padding: 20px; text-align: center; }\r\n" +
".email-body { padding: 20px; }\r\n" +
".email-footer { background: #f4f4f4; text-align: center; padding: 15px; font-size: 14px; color: #777; }\r\n" +
"</style>\r\n" +
"</head>\r\n" +
"<body>\r\n" +

"<div class=\"email-container\">\r\n" +

"    <div class=\"email-header\">\r\n" +
"        <h2>New Candidate Application Received</h2>\r\n" +
"    </div>\r\n" +

"    <div class=\"email-body\">\r\n" +

"        <p>Hello <strong>" + selfMasterDetailsViewModel.ContactPerson+ "</strong>,</p>\r\n" +

"        <p><strong>" + jobDetailsViewModel.CandidateApplication.Name + "</strong> has applied for the position of <strong>" +
(jobDetailsViewModel.tblJobPostDetail?.JobTitle ?? "N/A") + "</strong>.</p>\r\n" +

"        <p>You can review the candidate details and resume.</p>\r\n" +

"        <p>Applied on: " + DateTime.Now.ToString("dd MMM yyyy hh:mm tt") + "</p>\r\n" +

"        <br/>\r\n" +

"        <p>Regards,<br><strong>TheCloudTree System</strong></p>\r\n" +

"    </div>\r\n" +

"    <div class=\"email-footer\">\r\n" +
"        <p>thecloudtree.ai</p>\r\n" +
"        <p>Email: info@thecloudtree.ai | Phone: (+91) 9090973033</p>\r\n" +
"    </div>\r\n" +

"</div>\r\n" +
"</body>\r\n" +
"</html>";
                       
                        if(selfMasterDetailsViewModel.PrimaryEmail!=null)
                        {
                            SendEmail(selfMasterDetailsViewModel.PrimaryEmail, "New Job Application Received", Abody);
                        }

                        return RedirectToAction("Career");
                    }
                    else
                    {
                        TempData["ResultType"] = "Error";
                        TempData["ResultMessage"] = "Something went wrong! Please try again.";

                        ViewBag.OpenModal = true;
                        //var JobList = await GetAllJobList();
                        //jobDetailsViewModel.tblJobPostDetail =JobList.Where(e=>e.Jid==
                        jobDetailsViewModel.tblJobPostDetail = await GetJobById(jobDetailsViewModel.CandidateApplication.Jid);
                        return View("JobDetails", jobDetailsViewModel);
                    }
                }
                else
                {
                    TempData["ResultType"] = "Error";
                    TempData["ResultMessage"] = "Enter all required data";

                    ViewBag.OpenModal = true;
                    //jobDetailsViewModel.JobList = await GetAllJobList();

                    jobDetailsViewModel.tblJobPostDetail = await GetJobById(jobDetailsViewModel.CandidateApplication.Jid);
                    return View("JobDetails", jobDetailsViewModel);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "Error";
                TempData["ResultMessage"] = "Server error: " + ex.Message;

                ViewBag.OpenModal = true;
                //jobDetailsViewModel.JobList = await GetAllJobList();
                jobDetailsViewModel.tblJobPostDetail = await GetJobById(jobDetailsViewModel.CandidateApplication.Jid);
                return View("JobDetails", jobDetailsViewModel);

                //return View("JobDetails", new { Jid = jobDetailsViewModel?.tblJobPostDetail?.Jid });
            }
        }

        private async Task<TblJobPostDetail> GetJobById(int jid)
        {
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(
                    Common.GetBaseAddress() + "Recruitment/GetJobPostDetailsByJid/" + jid);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TblJobPostDetail>(apiResponse)
                           ?? new TblJobPostDetail();
                }
            }

            return new TblJobPostDetail();
        }

        public async Task<SelfMasterDetailsViewModel> SelfDetailsMaster()
        {
            commonService = new CommonServices();
            SelfMasterDetailsViewModel masterDetailsViewModel = new SelfMasterDetailsViewModel();
            ResponseModel responseModel = new ResponseModel();
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
        public IActionResult Customdevelopment()
        {
            return View();
        }
        [HttpGet("company/gallery", Name = "Gallery")]
        // for URL optimize--Anshuman 13.07.2026

        public async Task<IActionResult> Gallery()
        {
            List<GalleryMasterViewModel> list = new();
            var baseurlforimg = Common.GetUrlImageAndDocAddress() ?? "";
            using (var httpClient = Common.GetHttpClient())
            {
                var response = await httpClient.GetAsync(Common.GetBaseAddress() + "HCMPortalGallery/GetAllPortalGalleryList");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    list = JsonConvert.DeserializeObject<List<GalleryMasterViewModel>>(data) ?? new List<GalleryMasterViewModel>();
                }
            }
            ViewBag.baseurlforimg = baseurlforimg + "assets/UploadImg/";
            return View(list);
        }
        [HttpGet("company/investorcell", Name = "Investorcell")]
        // for URL optimize--Anshuman 13.07.2026
        public IActionResult Investorcell()
        {
            return View();
        }
        public IActionResult Letsconnect()
        {
            return View();
        }
        public IActionResult Partners()
        {
            return View();
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
        


        private async Task<bool> VerifyRecaptcha(string gRecaptchaResponse)
        {
            var secretKey = "6LcP1r4rAAAAAAUqp1ZfJeEqnAEJlG1m5FSwJnjF"; // ?? from Google Admin Console

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={gRecaptchaResponse}",
                null
            );

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            return result?.success == true;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(HCMEnquiryViewModel model, [FromForm(Name = "g-recaptcha-response")] string gRecaptchaResponse)
        {

            if (string.IsNullOrEmpty(gRecaptchaResponse) || !await VerifyRecaptcha(gRecaptchaResponse))
            {
                return Json(new
                {
                    success = false,
                    message = "Please verify reCAPTCHA."
                });
            }
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed. Please check fields." });
            }

            try
            {
                // ? RANDOM ID GENERATE
                model.EnquiryId = "HCMEI-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                using (var httpClient = Common.GetHttpClient())
                {
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(
                        Common.GetBaseAddress() + "HCMPortalEnquiry/SaveHcmenquiry",
                        stringContent
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        await SendAdminEmail(model);
                        await SendAcknowledgementEmail(model);
                        var notificationAdmin = new AmsnotificationTransactionViewModel()
                        {
                            UserId = 1,
                            SendMessage = $"New enquiry alert! {model.ProspectName} just reached out (ID: {model.EnquiryId}).",
                            Active = true,
                            SenderEmail = model.EmailAddress,
                            ReceiverEmail = "thecloudtree.developer@gmail.com",
                            SendBy = 0,
                            SendDate = DateTime.Now,
                        };
                        await _notificationService.SendMessage(notificationAdmin);
                        return Json(new
                        {
                            success = true,
                            message = "Message received successfully. Our team will contact you shortly."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Something went wrong while saving."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        public async Task SendAdminEmail(HCMEnquiryViewModel model)
        {
            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add("thecloudtree.developer@gmail.com");
                    msg.From = new MailAddress("thecloudtree.developer@gmail.com", "The CloudTree");
                    msg.ReplyToList.Add(new MailAddress(model.EmailAddress));
                    msg.Subject = model.Subject;
                    msg.Body = $@"
<div style='font-family: Arial; padding:20px;'>

<h2 style='color:#333;'>
    Enquiry from HCM Portal - {DateTime.Now.ToString("dd-MMM-yyyy")}
</h2>
    

    <p><b> Enquiry ID:</b> {model.EnquiryId}</p>

    <p>
        A new enquiry has been submitted through the portal.
        Please review the details below.
    </p>

    <hr/>

    <div style='white-space:pre-wrap; line-height:1.6;'>

<strong>Message:</strong>
{model.Message}

<br/><br/>

<strong>Name:</strong> {model.ProspectName} <br/>
<strong>Phone:</strong> {model.PhoneNumber} <br/>
<strong>Email:</strong> {model.EmailAddress}

    </div>

</div>";
                    msg.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("thecloudtree.developer@gmail.com", "jdndluquofjmwzjl");
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Admin Email Failed: " + ex.Message);
            }
        }
        public async Task SendAcknowledgementEmail(HCMEnquiryViewModel model)
        {
            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add(model.EmailAddress);

                    msg.From = new MailAddress("thecloudtree.developer@gmail.com", "The CloudTree");

                    msg.Subject = "We have received your enquiry";

                    msg.Body = $@"
<div style='font-family: Arial; padding:20px;'>

    <h2 style='color:#28a745;'>Thank You for Contacting Us!</h2>

    <p>Dear {model.ProspectName},</p>

    <p>
        Your request has been successfully submitted.
        Our team will review your enquiry and contact you shortly.
    </p>


    <hr/>

    <p>
        Regards,<br/>
        The CloudTree Team<br/>
        thecloudtree.developer@gmail.com
    </p>

</div>";

                    msg.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("thecloudtree.developer@gmail.com", "jdndluquofjmwzjl");
                        smtp.EnableSsl = true;

                        await smtp.SendMailAsync(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("User Email Failed: " + ex.Message);
            }
        }


        [HttpGet("company/about-us", Name ="AboutUs")]
        // for URL optimize--Anshuman 13.07.2026
        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet("company/contact-us", Name = "ContactUs")]
        // for URL optimize--Anshuman 13.07.2026
        public IActionResult ContactUs()
        {
            return View();
        }


        public IActionResult HCMFeatureTimeAndAttendance()
        {
            return View();
        }
        public IActionResult HCMFeaturePayrollManagement()
        {
            return View();
        }
        public IActionResult HCMFeatureHiringAndOnboarding()
        {
            return View();
        }
        public IActionResult HCMFeatureLeaveManagement()
        {
            return View();
        }
        public IActionResult HCMFeaturePerformanceAndCareer()
        {
            return View();
        }
        public IActionResult HCMFeatureExitsManagement()
        {
            return View();
        }
        public IActionResult SecurityTrust()
        {
            return View();
        }
        



    }
}
