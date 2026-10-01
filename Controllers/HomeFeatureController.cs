using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HCMPortal.Controllers
{
    public class HomeFeatureController : Controller
    {
        // GET: HomeFeatureController
        public ActionResult Index()
        {
            return View();
        }

        // GET: HomeFeatureController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeFeatureController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeFeatureController/Create
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

        // GET: HomeFeatureController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeFeatureController/Edit/5
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

        // GET: HomeFeatureController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeFeatureController/Delete/5
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
