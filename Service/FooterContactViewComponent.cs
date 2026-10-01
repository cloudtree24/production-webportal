using HCMPortal.Helper;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HCMPortal.Service
{
    public class FooterContactViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(int csid, string membershipid)
        {
            List<WebContactInfoViewModel> model = new List<WebContactInfoViewModel>();

            using (var httpClient = Common.GetHttpClient())
            {
                var url = Common.GetBaseAddress() +
                          $"WebContact/GetAllActiveWebContacts";

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    model = JsonConvert.DeserializeObject<List<WebContactInfoViewModel>>(json)
                            ?? new List<WebContactInfoViewModel>();

                }
            }

            return View(model);
        }
    }
}
