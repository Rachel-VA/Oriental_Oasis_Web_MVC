using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrientalOasis.DataAccess.Repository.IRepository;
using System.Security.Claims;
using OrientalOasis.Model;
using OrientalOasis.Utilities;
using OrientalOasis.Model.ViewModels;

namespace Oriental_Oasis_Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = StaticDetails.Role_Customer + "," + StaticDetails.Role_Company)]
    public class CustomerOrderHistoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerOrderHistoryController(IUnitOfWork unitOfWork)

        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult CustomerOrderHistory()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            if (orderVM.OrderHeader == null)
            {
                return NotFound();
            }

            return View(orderVM);
        }

        [HttpGet]
        public IActionResult GetCustomerOrders(string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();

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
                    break;
            }

            var result = orders.Select(order => new
            {
                order.Id,
                order.Name,
                Address = $"{order.StreetAddress}, {order.City}, {order.State}, {order.PostalCode}",
                order.PhoneNumber,
                order.OrderStatus,
                order.OrderTotal,
                order.Email
            }).ToList();

            return Json(new { data = result });
        }
    }
}
