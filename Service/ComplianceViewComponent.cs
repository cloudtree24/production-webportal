using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HCMPortal.Service
{
    public class ComplianceViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int csid, string membershipid)
        {
            List<DynamicPage> model = new List<DynamicPage>();

            using (var httpClient = Common.GetHttpClient())
            {
                var url = Common.GetBaseAddress() +
                          $"DynamicPage/GetAllPages";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<List<DynamicPage>>(json)
                            ?? new List<DynamicPage>();
                    model = model.Where(e => e.IsPublish == 1).ToList();
                }
            }

            return View(model);
        }
    }
}
