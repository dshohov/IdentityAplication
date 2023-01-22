using IdentityApp.Data;
using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityApp.Controllers
{
    public class UserController : Controller
    {
        public readonly UserManager<AppUser> _user;
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db, UserManager<AppUser> user)
        {
            _db = db;
            _user = user;
        }
        public IActionResult Index()
        {
            var userList = _db.AppUser.ToList();
            var roleList = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in userList)
            {
                var role = roleList.FirstOrDefault(x => x.UserId == user.Id);
                if(role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId).Name;
                }
            }
            return View(userList);   
        }

        [HttpGet]
        public IActionResult Edit(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if(user == null)
            {
                return NotFound();
            }

            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            var role = userRole.FirstOrDefault(u => u.UserId == user.Id);
            if(role != null)
            {
                user.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }
            user.RoleList = _db.Roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser user)
        {
            if (ModelState.IsValid)
            {
                var userDbValue = _db.AppUser.FirstOrDefault(u => u.Id == user.Id);
                if (userDbValue == null)
                {
                    return NotFound();
                }
                userDbValue.NickName = user.NickName;
                userDbValue.UserName = user.NickName;
                userDbValue.NormalizedUserName = user.NickName.ToUpper();

                var userRole = _db.UserRoles.FirstOrDefault(u => u.UserId == userDbValue.Id);
                if (userRole != null)
                {
                    var previousRoleName = _db.Roles.Where(u => u.Id == userRole.RoleId).Select(e => e.Name).FirstOrDefault();
                    await _user.RemoveFromRoleAsync(userDbValue, previousRoleName);

                }

                await _user.AddToRoleAsync(userDbValue, _db.Roles.FirstOrDefault(u => u.Id == user.RoleId).Name);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }


            user.RoleList = _db.Roles.Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });
            return View(user);
        }

        [HttpPost]
        public IActionResult Delete(string userId)
        {
            var user = _db.AppUser.FirstOrDefault(u => u.Id == userId);
            if(user == null)
            {
                return NotFound();
            }
            _db.AppUser.Remove(user);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
