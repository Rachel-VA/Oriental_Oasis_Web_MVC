using Microsoft.AspNetCore.Mvc;
using OrientalOasis.DataAccess.Repository.IRepository;
using System.Collections.Generic;
using System.Linq;
using OrientalOasis.Model;
using OrientalOasis.Utilities;
using OrientalOasis.Model.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Stripe;

namespace Oriental_Oasis_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM {  get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                // Retrieve order detail
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        // Only admin/employee can update order details
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderfromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            //map details want to update
            orderHeaderfromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderfromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderfromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderfromDb.City = OrderVM.OrderHeader.City;
            orderHeaderfromDb.State = OrderVM.OrderHeader.State;
            orderHeaderfromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
			orderHeaderfromDb.Email = OrderVM.OrderHeader.Email;

			//only update when empty
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderfromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderfromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderfromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated Sucessfully";

            return RedirectToAction(nameof(Details), new {orderId = orderHeaderfromDb.Id});
        }

        //method for start processing
        [HttpPost]

        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetails.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details has updated successfully";

            return RedirectToAction(nameof(Details), new {orderId = OrderVM.OrderHeader.Id});
        }


        //method for start ship order
        [HttpPost]

        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = orderVM.OrderHeader.OrderStatus;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
                      
            TempData["success"] = "Order has shipped";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        //method for start cancel order
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusApproved) 
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetails.StatusCancelled, StaticDetails.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["success"] = "Order cancelled";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }




        #region API CALLS
        [HttpGet]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> orders;

            //only admin and employee roles can see all order details
            if (User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee))
            {
                orders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }

            switch (status)
            {
                case "pending":
                    orders = orders.Where(u => u.OrderStatus == StaticDetails.PaymentStatusDelayedPayment).ToList();
                    break;
                case "inprocess":
                    orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusInProcess).ToList();
                    break;
                case "completed":
                    orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusShipped).ToList();
                    break;
                case "approved":
                    orders = orders.Where(u => u.OrderStatus == StaticDetails.StatusApproved).ToList();
                    break;
                default:
                    // No filtering
                    break;
            }

            var result = orders.Select(order => new
            {
                order.Id,
                order.Name,
                Address = $"{order.StreetAddress}, {order.City}, {order.State}, {order.PostalCode}", // Concatenated address
                order.PhoneNumber,
                order.OrderStatus,
                order.OrderTotal,
                order.Email
                // add more properties here if needed
            }).ToList();

            return Json(new { data = result });
        }
        #endregion

    }
}
