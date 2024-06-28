using Microsoft.AspNetCore.Mvc;
using OrientalOasis.Model;
using System.Diagnostics;
using OrientalOasis.DataAccess.Data;
using OrientalOasis.Utilities;
//using OrientalOasis.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Logging;
using OrientalOasis.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
//using OrientalOasis.Utilities;
//using OrientalOasis.DataAcess.Repository.IRepository; // This is important


namespace Oriental_Oasis_Web.Areas.Admin.Controllers
{
    //using attribute Area to define specific Area for the controller
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]

    public class CategoryController : Controller
    {
        //impletementation for DbContext which registered in Program.cs
        private readonly IUnitOfWork _unitWork;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(IUnitOfWork unitWork, ILogger<CategoryController> logger)
        {
            _unitWork = unitWork;
            _logger = logger;
        }
        public IActionResult Index()
        {
            //inherent from Entiry Framework. set it to the Cateegories to retrive data from the database
            List<Category> objCategoryList = _unitWork.Category.GetAll().ToList();
            return View(objCategoryList); //pass the CatList into the view and capture it into the index.cshtml file
        }

        //create a new action method for a new view Create. By default the IAction method is a the get method
        public IActionResult Create()
        {
            return View();
        }
        //create an action method Post for the Create page
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitWork.Category.Add(obj);
                    _unitWork.Save(); // Save changes to the database
                    TempData["success"] = "New category created successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes to the database");
                    ModelState.AddModelError("", "Unable to save changes. Try again");
                }
            }
            return View();
        }//end Post method


        //*************** create/copy the Get and Post method above to create an Edit view************
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitWork.Category.Get(u => u.Cat_Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }


        //create an action method Post for the Edit page
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitWork.Category.Update(obj);
                    _unitWork.Save(); // Save changes to the database
                    TempData["success"] = "Category Updated successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes to the database");
                    ModelState.AddModelError("", "Unable to save changes. Try again");
                }
            }
            return View();
        }//end Edit


        //*************** create/copy the Get and Post method above to create an Delete view************
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitWork.Category.Get(u => u.Cat_Id == id);
            //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Cat_Id==id).FirstOrderDefault();
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }


        //create an action method Post for the Edit page
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null || id == 0)
            { return NotFound(); }

            //find category from database
            Category? obj = _unitWork.Category.Get(u => u.Cat_Id == id);
            if (obj == null)
            {
                return NotFound();

            }
            try
            {
                //calling Remove method to remove obj
                _unitWork.Category.Delete(obj);
                _unitWork.Save(); // Save changes to the database
                TempData["success"] = "Category deleted successfully";
                _logger.LogInformation($"Category with id {id} deleted successfully");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category with id {id}");
                return View(obj);
            }
            return RedirectToAction("Index");


        }//end Delete

    }//end class


}
