using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleMVC.DataAccess.Repository.IRepository;
using SampleMVC.Models;
using SampleMVC.Models.ViewModels;
using SampleMVC.Utility;
using SampleMVCApp.DataAccess.Data;
using System.Collections.Generic;

namespace SampleMVCApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagment(string userId)
        {
            string RoleId = _db.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;

            RolemanagmentVM RoleVM = new RolemanagmentVM()
            {
                ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }), 
                CompanyList = _db.Companies.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RolemanagmentVM rolemanagmentVM)
        {
            string RoleId = _db.UserRoles.FirstOrDefault(x => x.UserId == rolemanagmentVM.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

            if (!(rolemanagmentVM.ApplicationUser.Role == oldRole))
            {
                // a role was updated
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == rolemanagmentVM.ApplicationUser.Id);
                if (rolemanagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = rolemanagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, rolemanagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();
                TempData["success"] = $"Role Changed to {rolemanagmentVM.ApplicationUser.Role} successfully";
            }

            return RedirectToAction("Index");
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

            List<IdentityUserRole<string>> userRoles = _db.UserRoles.ToList();
            List<IdentityRole> roles = _db.Roles.ToList();

            foreach (ApplicationUser user in objUserList)
            {
                string roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                if (user.Company == null)
                {
                    user.Company = new Company() { Name = ""};
                }
            }

            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successfully." });
        }
        #endregion
    }
}
