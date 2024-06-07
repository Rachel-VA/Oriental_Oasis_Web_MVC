using Microsoft.AspNetCore.Mvc;
using Oriental_Oasis_Web.Models;
using Oriental_Oasis_Web.Data;
using Microsoft.Extensions.Logging; // Add this if not already present
//using OrientalOasis.DataAcess.Repository.IRepository; // This is important


namespace Oriental_Oasis_Web.Controllers
{
    public class CategoryController : Controller
    {
        //impletementation for DbContext which registered in Program.cs
        private readonly ApplicationDbContext _db;
		private readonly ILogger<CategoryController> _logger;
		public CategoryController(ApplicationDbContext db, ILogger<CategoryController> logger)
		{
			_db=db;
			_logger = logger;
		}
		public IActionResult Index()
        {
            //inherent from Entiry Framework. set it to the Cateegories to retrive data from the database
            List<Category>objCategoryList = _db.Categories.ToList();
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
					_db.Categories.Add(obj);
					_db.SaveChanges(); // Save changes to the database
					TempData["success"] = "New category created successfully";
					return RedirectToAction("Index");
				}
                catch(Exception ex)
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
			if(id == null || id == 0)
			{
				return NotFound();
			}
			Category? categoryFromDb = _db.Categories.Find(id);
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
					_db.Update(obj);
					_db.SaveChanges(); // Save changes to the database
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
			Category? categoryFromDb = _db.Categories.Find(id);
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
			Category? obj = _db.Categories.Find(id);
			if (obj == null) 
			{ 
				return NotFound();
			
			}
			try
			{
				//calling Remove method to remove obj
				_db.Categories.Remove(obj);
				_db.SaveChanges(); // Save changes to the database
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
