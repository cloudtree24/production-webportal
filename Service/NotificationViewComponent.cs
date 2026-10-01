using HCMPortal.Helper;
using HCMPortal.Models;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace HCMPortal.Service
{
    public class NotificationViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<TblCrmcommunicationTransaction> Notifications = new List<TblCrmcommunicationTransaction>();
            string emailAddress = HttpContext.Session.GetString("UserName")?? string.Empty;
            try
            {
                if (emailAddress!=string.Empty && emailAddress!=null)
                {
                    using (var httpClient = Common.GetHttpClient())
                    {
                        //using (var response = httpClient.GetAsync(Common.GetBaseAddress() + "Email/GetAllNotificationByEmployeeId/" + model.ChildCompanyId+"/" + model.EmployeeId))
                        using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "CRMCustomerManagement/GetAllNotification/" + emailAddress))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                Notifications = JsonConvert.DeserializeObject<List<TblCrmcommunicationTransaction>>(apiResponse)??new List<TblCrmcommunicationTransaction>();
                                
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
                return View(new List<TblCrmcommunicationTransaction>());
            }
            return View(Notifications);
        }
    }
}
