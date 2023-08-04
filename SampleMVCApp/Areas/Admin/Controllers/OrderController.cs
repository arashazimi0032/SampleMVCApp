﻿using Microsoft.AspNetCore.Mvc;
using SampleMVC.DataAccess.Repository.IRepository;
using SampleMVC.Models;
using SampleMVC.Models.ViewModels;
using SampleMVC.Utility;
using System.Diagnostics;

namespace SampleMVCApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

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
			OrderVM orderVM = new OrderVM
			{
				OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties:"ApplicationUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties:"Product")
			};
			return View(orderVM);
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objOrderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

			switch (status)
			{

                case "pending":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

			return Json(new { data = objOrderHeader });
		}
		#endregion
	}
}
