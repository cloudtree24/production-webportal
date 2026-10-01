using HCMPortal.Helper;
using HCMPortal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace HCMPortal.Service
{
    public class OurTeamViewComponent : ViewComponent
    {
        private readonly IWebHostEnvironment _hostingenvironment;

        public OurTeamViewComponent(IWebHostEnvironment hostingenvironment)
        {
            _hostingenvironment = hostingenvironment;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<TblExecutiveDetail> tblExecutiveDetail = new();

            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(
                    Common.GetBaseAddress() + "OurTeam/GetAllOurTeamList"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        tblExecutiveDetail =
                            JsonConvert.DeserializeObject<List<TblExecutiveDetail>>(apiResponse)
                            ?? new List<TblExecutiveDetail>();
                    }
                }
            }

            foreach (var item in tblExecutiveDetail)
            {
                item.ExecutiveImage = GetImageUrl(item.ExecutiveImage);

                item.Description = UpdateDescriptionImages(item.Description);
            }

            return View(tblExecutiveDetail);
        }

        private string GetImageUrl(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return "/assets/images/user_icon.png";

            if (imageName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return imageName;

            string fileName = Path.GetFileName(imageName);

            string relativePath = Path.Combine("assets", "UploadImg", fileName);

            string absolutePath = Path.Combine(
                _hostingenvironment.WebRootPath,
                relativePath);

            if (System.IO.File.Exists(absolutePath))
            {
                return "/" + relativePath.Replace("\\", "/");
            }

            return Common.ImageAndDocSavePageAddress()
                    + "assets/UploadImg/"
                    + fileName;
        }

        private string UpdateDescriptionImages(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            return Regex.Replace(
                html,
                "<img[^>]*src=[\"']([^\"']+)[\"'][^>]*>",
                match =>
                {
                    string src = match.Groups[1].Value;

                    string newSrc = GetImageUrl(src);

                    return match.Value.Replace(src, newSrc);

                },
                RegexOptions.IgnoreCase);
        }
    }
}