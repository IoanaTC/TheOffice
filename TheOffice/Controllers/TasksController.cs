using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Data;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using TheOffice.Data;
using TheOffice.Models;
using Task = TheOffice.Models.Task;

namespace TheOffice.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        
        private IWebHostEnvironment _env;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env)
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;
        }

        // Se afiseaza un singur task in functie de id-ul sau 
        // impreuna cu titlul proiectului din care face parte
        // bsi toate comentariile asociate unui task
        // Se afiseaza si userul care are de rezolvat task-ul respectiv
        // HttpGet implicit

        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult Show(int? id)
        {
            Task task = db.Tasks.Include("Project")
                                .Include("Comments")
                                .Include("Comments.User")
                                .Include("Status")
                                .Include("User")
                                .Where(tsk => tsk.Id == id)
                                 .First();

            var members = new List<string>();

            var project_users = db.UserProjects.Include("User")
                                .Where(up => up.ProjectId == task.ProjectId);

            foreach (UserProject user in project_users)
                members.Add(user.UserId);

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"].ToString();
            }

            if (members.Contains(_userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(task);
            }

            else
            {
                TempData["message"] = "Nu aveti acces la acest task!";
                return Redirect("/Projects/Index/");
            }
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
                                     .Include("Status")
                                     .Include("Comments.User")
                                     .Where(tsk => tsk.Id == comment.TaskId)
                                     .First();

                SetAccessRights();

                return View(myTask);
            }
        }

        // Se afiseaza formularul in care se vor completa datele unui task
        // HttpGet implicit

        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult New()
        {
            int projectid = Convert.ToInt32(HttpContext.Request.Query["project"]);
            var project = db.Projects.Find(projectid);


            // verificare daca userul e admin sau organizatorul proiectului curent
            if (User.IsInRole("Admin") || project.OrganizatorId == _userManager.GetUserId(User))
            {
                Task task = new Task();
                task.ProjectId = projectid;

                task.Stat = GetAllStatuses();
                return View(task);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa creati un nou task!";
                return Redirect("/Projects/Show/" + projectid);
            }
        }

        // Se adauga task-ul in baza de date
        // Doar utilizatorii cu rolul de Organizator sau Admin pot adauga task-uri

        [Authorize(Roles = "Organizator,Admin")]
        [HttpPost]
        public IActionResult New(Task task)
        {
            var project = db.Projects.Find(task.ProjectId);

            //verificare daca organizatorul care creeaza task-ul este organizatorul proiectului respectiv
            if (User.IsInRole("Admin") || project.OrganizatorId == _userManager.GetUserId(User))
            {
                task.StatusId = 1;
                task.StartDate = null;
                task.Stat = GetAllStatuses();

                if (ModelState.IsValid)
                {
                    db.Tasks.Add(task);
                    db.SaveChanges();
                    TempData["message"] = "Task-ul a fost adaugat";
                    return Redirect("/Tasks/Show/" + task.Id);
                }
                else
                {
                    task.Stat = GetAllStatuses();
                    return View(task);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa creati un nou task!";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
        }

        // Se editeaza un task existent in baza de date
        // HttpGet implicit
        // Se afiseaza formularul impreuna cu datele aferente task-ului din baza de date
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult Edit(int id)
        {

            Task task = db.Tasks.Include("Comments")
                                 .Include("Project")
                                 .Where(tsk => tsk.Id == id)
                                 .First();

            task.Stat = GetAllStatuses();

            if (task.Project.OrganizatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(task);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui task!";
                return Redirect("/Projects/Index/");
            }

        }

        // Se adauga task-ul modificat in baza de date
        [HttpPost]
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult Edit(int id, Task requestTask)
        {
            Task task = db.Tasks.Include("Project")
                                        .Where(tsk => tsk.Id == id)
                                        .First();
            task.Stat = GetAllStatuses();
            if (task.Project.OrganizatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    task.Title = requestTask.Title;
                    task.Content = requestTask.Content;
                    task.Deadline = requestTask.Deadline;
                    TempData["message"] = "Task-ul a fost modificat";
                    db.SaveChanges();
                    return Redirect("/Tasks/Show/" + id);
                }
                else
                {
                    task.Stat = GetAllStatuses();
                    return View(requestTask);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra acestui task!";
                return Redirect("/Projects/Index/");
            }
        }

        [Authorize(Roles = "User,Organizator,Admin")]
        public ActionResult EditStatus(int id)
        {
            //Task task  = new Task();
            Task task = db.Tasks.Include("Status")
                                        .Where(tsk => tsk.Id == id)
                                        .First();

            // Se preia lista de statusuri 
            task.Stat = GetAllStatuses();

            //preiau membrii proiectului respectiv
            var members = new List<string>();

            var project_users = db.UserProjects.Include("User")
                                .Where(up => up.ProjectId == task.ProjectId);

            foreach (UserProject user in project_users)
                members.Add(user.UserId);

            if (members.Contains(_userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                return View(task);
            }

            else
            {
                TempData["message"] = "Nu aveti acces sa editati statusul!";
                return Redirect("/Projects/Index/");
            }
        }

        // Se modifica statusul cu noua valoare
        [HttpPost]
        [Authorize(Roles = "User,Organizator,Admin")]
        public IActionResult EditStatus(int id, Task requestTask)
        {
            Task task = db.Tasks.Include("Status")
                                        .Where(tsk => tsk.Id == id)
                                        .First();
            
            task.Stat = GetAllStatuses();

            var members = new List<string>();

            var project_users = db.UserProjects.Include("User")
                                .Where(up => up.ProjectId == task.ProjectId);

            foreach (UserProject user in project_users)
                members.Add(user.UserId);

            if (members.Contains(_userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                if (requestTask.StatusId != null)
                {
                    task.StatusId = requestTask.StatusId;

                    //adaugam modificarea id-ului statusului in baza de date
                    db.SaveChanges();  

                    //setam data de start in functie de acesta
                    if (task.Status.Status_Value == "In Progress")
                    {
                        task.StartDate = DateTime.Now;

                    }

                    if (task.Status.Status_Value == "Not Started")  // Back to Not Started
                    {
                        task.StartDate = null;
                    }
                    db.SaveChanges();
                    TempData["message"] = "Statusul a fost modificat!";
                    return Redirect("/Tasks/Show/" + task.Id);
                }
                else
                {
                    requestTask.Stat = GetAllStatuses();
                    TempData["message"] = "Va rugam sa alegeti un status!";
                    ViewBag.Msg = TempData["message"].ToString();
                    return View(requestTask);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti acces sa editati statusul!";
                return Redirect("/Projects/Index/");
            }
        }


        // Se sterge un task din baza de date 
        [Authorize(Roles = "Organizator,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Include("Comments")
                                 .Include("Project")
                                 .Where(tsk => tsk.Id == id)
                                 .First();
            if (task.Project.OrganizatorId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "Task-ul a fost sters cu succes!";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti acest task!";
                return Redirect("/Projects/Index/");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStatuses()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate statusurile din baza de date
            var statuses = from stat in db.Statuses
                           select stat;

            // iteram prin statusuri
            foreach (var status in statuses)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul statusului si valoarea acestuia
                selectList.Add(new SelectListItem
                {
                    Value = status.Id.ToString(),
                    Text = status.Status_Value.ToString()
                });
            }

            // returnam lista de statusuri
            return selectList;
        }

        // Conditiile de afisare a butoanelor de editare si stergere
        [NonAction]
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Organizator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }
    }
}

