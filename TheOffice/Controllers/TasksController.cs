using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TheOffice.Data;
using TheOffice.Models;
using Task = TheOffice.Models.Task;

namespace TheOffice.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Adaugarea unui comentariu asociat unui task
        [HttpPost]
        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Tasks/Show/" + comment.TaskId);
            }

            else
            {
                Task myTask = db.Tasks.Include("User")
                                      .Include("Project")
                                     .Include("Comments")
                                     .Include("Comments.User")
                                     .Where(art => art.Id == comment.TaskId)
                                     .First();

                //SetAccessRights();    //pentru butoane mai tarziu

                return View(myTask);
            }
        }
    }
}
