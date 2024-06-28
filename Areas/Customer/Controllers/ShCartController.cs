using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientalOasis.DataAccess.Repository.IRepository;
using OrientalOasis.Model;
using OrientalOasis.Model.ViewModels;
using OrientalOasis.Utilities;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Oriental_Oasis_Web.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class ShCartController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        //bind property to use ShoppingCartVM for both area
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ShCartController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public IActionResult Index()
        {
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }

            return View(ShoppingCartVM);
        }

        //method for order summary
        public ActionResult Summary() {
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			if (userId == null)
			{
				// Handle the case where userId is null
				return RedirectToAction("Index", "Home");
			}

			ShoppingCartVM = new ShoppingCartVM
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new OrderHeader()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser =_unitOfWork.ApplicationUser.Get(u=>u.Id == userId);
            if (ShoppingCartVM.OrderHeader.ApplicationUser != null) {

                //populate user info from ApplicationUser and update info for shipping info 
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.Email = ShoppingCartVM.OrderHeader.ApplicationUser.Email;

            }          
            //calculate the total in shopping cart
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        
        }

        //*****************************************************************

        //Post action method for order summary
        // Post action method for order summary
        [HttpPost]
        [ActionName("Summary")]
        public ActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Handle the case where userId is null or empty
                return RedirectToAction("Index", "Home");
            }

            
           ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            if (applicationUser == null)
            {
                // Handle the case where applicationUser is null
                return RedirectToAction("Index", "Home");
            }


            // Set the email from ApplicationUser
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.Email = applicationUser.Email; // Ensure this line correctly uses the instantiated applicationUser


            // Calculate the total in shopping cart
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Capture payment for regular account
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            else
            {
                // Company user
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
            }

            // Order header/info
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Stripe logic for payment
                var domain = "https://localhost:7218/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/shcart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/shcart/index", //go back to shopping cart
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var SessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.5 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.ItemName
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(SessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateSripePaymentId(ShoppingCartVM.OrderHeader.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        //****************************************************

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                // Handle the case where the order header is not found
                return NotFound();
            }

            // Payment delay
            if (orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
            {
                if (!string.IsNullOrEmpty(orderHeader.SessionId))
                {
                    var service = new SessionService();
                    try
                    {
                        Session session = service.Get(orderHeader.SessionId);

                        if (session.PaymentStatus.ToLower() == "paid")
                        {
                            _unitOfWork.OrderHeader.UpdateSripePaymentId(id, session.Id, session.PaymentIntentId);
                            _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                            _unitOfWork.Save();
                        }
                        HttpContext.Session.Clear();
                    }
                    catch (StripeException ex)
                    {
                        // Log the exception 
                        Console.WriteLine($"Stripe error: {ex.Message}");
                        
                    }
                }
                else
                {
                    // case where SessionId is null or empty
                    Console.WriteLine("Order session ID is null or empty.");
                }
            }

            // Remove shopping cart/empty
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
            _unitOfWork.Save();

            // Return the view
            return View("/Areas/Customer/Views/ShCart/OrderConfirmation.cshtml", id);
        }


        //incease item in shoping cart action method
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();

                var userId = cartFromDb.ApplicationUserId;
                HttpContext.Session.SetInt32(StaticDetails.SessionShCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).ToList().Sum(c => c.Count));
            }

            return RedirectToAction(nameof(Index));
        }


        //decreate method
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb != null)
            {
                var userId = cartFromDb.ApplicationUserId;

                if (cartFromDb.Count <= 1)
                {
                    //remove item from cart
                    _unitOfWork.ShoppingCart.Delete(cartFromDb);
                }
                else
                {
                    cartFromDb.Count -= 1;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }

                _unitOfWork.Save();

                HttpContext.Session.SetInt32(StaticDetails.SessionShCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).ToList().Sum(c => c.Count));
            }

            return RedirectToAction(nameof(Index));
        }




        //Delete method
        public IActionResult Delete(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);

            if (cartFromDb != null)
            {
                var userId = cartFromDb.ApplicationUserId;
                _unitOfWork.ShoppingCart.Delete(cartFromDb);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(StaticDetails.SessionShCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).ToList().Sum(c => c.Count));
            }

            return RedirectToAction(nameof(Index));
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }


        }
    }
}
