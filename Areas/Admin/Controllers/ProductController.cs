using Microsoft.AspNetCore.Mvc;
using OrientalOasis.Model;
using OrientalOasis.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrientalOasis.Model.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using OrientalOasis.Utilities;

namespace Oriental_Oasis_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<ProductController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            _logger.LogInformation($"Number of products: {objProductList.Count}");
            foreach (var product in objProductList)
            {
                _logger.LogInformation($"Product: {product.ItemName}, ImageURL: {product.ImageURL}");
            }
            return View(objProductList);
        }

        public IActionResult UpSert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Cat_Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                // Create product
                return View(productVM);
            }
            else
            {
                // Update product
                productVM.Product = _unitOfWork.Product.Get(u => u.ProductId == id, includeProperties: "Category");
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult UpSert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                bool isNewProduct = productVM.Product.ProductId == 0;

                try
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath; // Access the root folder
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"IMGs\imgproduct");

                        if (!string.IsNullOrEmpty(productVM.Product.ImageURL))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageURL.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productVM.Product.ImageURL = @"\IMGs\imgproduct\" + fileName;
                        _logger.LogInformation($"Image uploaded: {productVM.Product.ImageURL}");
                    }

                    // Strip HTML tags from the description
                    productVM.Product.Description = Regex.Replace(productVM.Product.Description, "<.*?>", String.Empty);

                    if (isNewProduct)
                    {
                        _unitOfWork.Product.Add(productVM.Product);
                        _logger.LogInformation("Product added.");
                        TempData["success"] = "Product created successfully";
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                        _logger.LogInformation("Product updated.");
                        TempData["success"] = "Product updated successfully";
                    }

                    _unitOfWork.Save(); // Save changes to the database
                    _logger.LogInformation("Changes saved to the database.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes to the database");
                    ModelState.AddModelError("", "Unable to save changes. Try again");
                }
            }
            productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Cat_Id.ToString()
            });
            return View(productVM);
        }

        //create an API call to use plug in datatable which support my MVC
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.ProductId == id);

            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageURL.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Delete(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted successfully" });
        }

        #endregion
    }
}
