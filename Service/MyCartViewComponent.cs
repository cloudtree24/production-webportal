using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace HCMPortal.Service
{
    public class MyCartViewComponent:ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<MyCartViewModel> myCartViewModels = new List<MyCartViewModel>();
            int Csid = HttpContext.Session.GetInt32("UserId")??0;
            try
            {
                if (Csid>0)
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        //using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "Email/GetAllNotificationByEmployeeId/" + model.ChildCompanyId+"/" + model.EmployeeId))
                        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "OrderMaster/GetMyCart/" +Csid))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                myCartViewModels = JsonConvert.DeserializeObject<List<MyCartViewModel>>(apiResponse)??new List<MyCartViewModel>();
                                
                            }
                            //if(myCartViewModels.Count()>0)
                            //{
                            //    bool isEsist = true;
                            //}
                        }

                    }
                   
                }
            }
            catch 
            {
                return View(new List<MyCartViewModel>());
            }
            return View(myCartViewModels);
        }
    }
}
