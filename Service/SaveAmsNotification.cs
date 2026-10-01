using HCMPortal.Helper;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HCMPortal.Service
{
    public class SaveAmsNotification
    {
        public async Task<bool> SendMessage(AmsnotificationTransactionViewModel model)
        {
            try
            {
                using (var httpClient = Common.GetHttpClient())
                {
                    var stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(Common.GetBaseAddress() + "AMSNotification/SaveAllAmsNotifications",stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}