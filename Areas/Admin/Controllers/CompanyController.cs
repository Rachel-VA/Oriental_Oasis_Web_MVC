using Microsoft.AspNetCore.Mvc;
using OrientalOasis.Model;
using OrientalOasis.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using OrientalOasis.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Oriental_Oasis_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpSert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create
                return View(new Company());
            }
            else
            {
                // Update 
                Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company saved successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(companyObj);
            }
        }

        // Create an API call to use plug in DataTable which supports MVC
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _unitOfWork.Company.GetAll().Select(c => new
            {
                id = c.Id,
                name = c.Name,
                streetAddress = c.StreetAddress,
                city = c.City,
                state = c.State,
                postalCode = c.PostalCode,
                phoneNumber = c.PhoneNumber
            }).ToList();

            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting company" });
            }

            _unitOfWork.Company.Delete(companyToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Company deleted successfully" });
        }
        #endregion
    }
}
