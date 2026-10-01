using HCMPortal.Models;
using HCMPortal.Service;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HCMPortal.Helper;
using HCMPortal.ViewModel;

namespace HCMPortal.Controllers
{
    public class DynamicPageController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private static List<string> attachementpaths = new List<string>();
        public CommonServices commonService;
        public DynamicPageController(IWebHostEnvironment env)
        {
            _env = env;
        }


        //[HttpGet]
        //public async Task<IActionResult> Page(string slug)
        //{
        //    DynamicPage page = new DynamicPage();
        //    DynamicPageViewModel pageviewModel = new DynamicPageViewModel();
        //    using (var httpClient = Common.GetHttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DynamicPage/GetPageBySlug/" + slug))
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string apiResponse = await response.Content.ReadAsStringAsync();
        //                pageviewModel = JsonConvert.DeserializeObject<DynamicPageViewModel>(apiResponse) ?? new DynamicPageViewModel();
        //                //page.OldSlug = page.Slug;
        //                //page.IsReadyToPublish = (page.IsPublish == 1) ? true : false;
        //            }
        //            else
        //            {
        //                string error = await response.Content.ReadAsStringAsync();
        //            }
        //        }
        //    }
        //    //if page is for blog Detials 
        //    if(pageviewModel.PageType=="Blog")
        //    {
        //        //return RedirectToAction("Blogdetails", "Home", pageviewModel);
        //        //return RedirectToAction("Blogdetails", "Home", new { slug = pageviewModel.Slug });
        //        List<BlogViewModel> blogList = new List<BlogViewModel>();
        //        CommonServices commonServices = new CommonServices();
        //        List<BlogViewModel> related = new List<BlogViewModel>();
        //        related = await commonServices.GetAllActiveBlogList();
        //        //DynamicPageViewModel pageViewModel = related.Where(e => e.WebSlugName == slug).Select(e => new DynamicPageViewModel()
        //        //{
        //        //    Title = e.PostTitle,
        //        //    Slug = e.WebSlugName,
        //        //    MetaTitle = e.PostTitle,
        //        //    HtmlContent = e.Description,
        //        //    BannerImagePath = e.ImagePath,
        //        //    Category = e.Category,
        //        //    PageType = "Blog",
        //        //    CreatedDate = e.CreatedDate,
        //        //    ViewsCount = e.ViewsCount
        //        //}).FirstOrDefault() ?? new DynamicPageViewModel();

        //        if (!String.IsNullOrEmpty(pageviewModel.HtmlContent))
        //        {
        //            blogList = (related).Where(e => e.WebSlugName != pageviewModel.Slug && e.Category == pageviewModel.Category).ToList();
        //            if (blogList.Count() == 0)
        //            {
        //                blogList = related;
        //            }
        //            pageviewModel.relatedBlogs = blogList.Where(e => e.WebSlugName != pageviewModel.Slug).Take(5).ToList();
        //        }
        //        else
        //        {
        //            pageviewModel.HtmlContent = "<p>No Content Found</p>";
        //        }
        //    }
        //    return View(pageviewModel);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FileUpload(IFormFile[] files, string[] ids, string[] types)
        {
            var uploadedFileUrls = new List<object>();
            var webRootPath = _env.WebRootPath;

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var placeholderId = ids[i];
                var fileType = types[i]; // Get the file type from the request

                if (file.Length > 0)
                {
                    // Determine the subfolder based on the file type
                    string subfolder = "others";
                    string fileFormat = "";
                    if (fileType.StartsWith("image/"))
                    {
                        subfolder = "assets/UploadImg/";
                        fileFormat = "IMG";
                    }
                    else if (fileType.StartsWith("video/"))
                    {
                        subfolder = "assets/UploadVideo/";
                        fileFormat = "VIDEO";
                    }

                    var uploadsFolder = Path.Combine(webRootPath, subfolder);

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    Random rnd = new Random();
                    long firstNumber = rnd.NextInt64(100000000000, 1000000000000); // 12 digits
                    // Generate a unique filename and save the file
                    var uniqueFileName = firstNumber.ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    attachementpaths.Add(uniqueFileName);
                    var fileUrl = Path.Combine("/", subfolder, uniqueFileName).Replace("\\", "/");
                    uploadedFileUrls.Add(new { id = placeholderId, fileformat = fileFormat, url = fileUrl });
                }
            }

            return Json(uploadedFileUrls);
        }

        [HttpGet]
        public async Task<JsonResult> IsSlugExist(string Slug, string OldSlug)
        {
            ResponseModel responseModel = new ResponseModel();
            commonService = new CommonServices();
            try
            {
                if (!Slug.Equals(OldSlug))
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "DuplicateCheck/IsUrlSlugExist/" + Slug))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                responseModel = JsonConvert.DeserializeObject<ResponseModel>(apiResponse) ?? new ResponseModel();
                            }
                        }
                    }
                }
                return Json(!responseModel?.IsExist);
            }
            catch (Exception)
            {
                return Json(!responseModel?.IsExist);
            }
        }

        /// <summary>
        /// to add the like count for the blog--Jayanta(10/07/2026)
        /// </summary>
        /// <param name="blogId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddLike(string  slugName)
        {
            using (var client = Common.GetHttpClient())
            {
                var response = await client.PostAsync(
                    Common.GetBaseAddress() +
                    $"DynamicPage/AddLike?slugName={slugName}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }
    }
}
