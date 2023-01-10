using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TheOffice.Data;
using TheOffice.Models;

namespace TheOffice.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        // Stergerea unui comentariu asociat unui task existent in baza de date
        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                TempData["message"] = "Comentariu sters cu succes!";
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul de a sterge comentariul!";
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
        }

        // Se editeaza un comentariu existent(intr-o pagina separata)
        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(comm);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa modificati acest comentariu!";
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult Edit(int id, Comment requestComment)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    comm.Content = requestComment.Content;

                    db.SaveChanges();

                    TempData["message"] = "Comentariu editat!";

                    return Redirect("/Tasks/Show/" + comm.TaskId);
                }
                else
                {
                    return View(requestComment);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa modificati comentariul!";
                return Redirect("/Tasks/Show/" + comm.TaskId);
            }
        }
    }
}

