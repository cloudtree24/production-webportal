using HCMPortal.Helper;
using HCMPortal.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HCMPortal.Controllers
{
    public class TrainingController : Controller
    {
        // GET: TrainingController
        [HttpGet("customer/training", Name = "CustomerTraining")]
        public async Task<ActionResult> TrainingDetails()
        {
            List<TrainingViewModel> trainingViewModel = new List<TrainingViewModel>();
            using (var httpClient = Common.GetHttpClient())
            {
                using (var response = await httpClient.GetAsync(Common.GetBaseAddress() + "Training/GetAllTrainingModuleList"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        trainingViewModel = JsonConvert.DeserializeObject<List<TrainingViewModel>>(apiResponse) ?? new List<TrainingViewModel>();
                    }
                }
            }
            return View(trainingViewModel);
        }

        // GET: TrainingController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TrainingController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TrainingController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TrainingController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TrainingController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TrainingController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TrainingController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
