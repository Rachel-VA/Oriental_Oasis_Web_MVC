using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientalOasis.DataAccess.Repository.IRepository;
using OrientalOasis.Model; // Corrected namespace // Updated namespace
using System.Security.Claims;

//using OrientalOasis.Model.Models;
using System.Diagnostics;

namespace Oriental_Oasis_Web.Areas.Customer.Controllers
{
    //using Area attribute to specific area for the controller
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork; 
        
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties:"Category");
            return View(productList);
        }

        //handle logic for Detail Page
        public IActionResult Details(int ProductId)
        {
            //create a shoppingcart object
            ShoppingCart cart = new()
            {
              Product = _unitOfWork.Product.Get(u=>u.ProductId==ProductId,includeProperties: "Category"),
              Count=1,
              ProductId=ProductId,
            };

            
            return View(cart);
        }

        //handle logic for adding a tem to shoppingcart
        [HttpPost]
        [Authorize] //log in 1st
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //get user id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId  = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId == userId &&
            
            u.ProductId==shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                // shopping cart exist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);

            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);

            }
            TempData["success"] = " Your Shopping cart is sucessfully updated";




            _unitOfWork.ShoppingCart.Add(shoppingCart);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
