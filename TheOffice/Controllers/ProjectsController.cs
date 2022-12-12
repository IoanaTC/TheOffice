using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheOffice.Data;
using TheOffice.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Task = TheOffice.Models.Task;
using System.Diagnostics.Eventing.Reader;
using System.Security.Policy;
using System.Text.Encodings.Web;

namespace TheOffice.Controllers
{
    public class ProjectsController : Controller
    {
        // conexiunea cu baza de date
        private readonly ApplicationDbContext db;
        // managerul de useri si roluri
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // manager de fisiere
        private IWebHostEnvironment _env;

        // userul curent
        private readonly string currentUserId;

        public ProjectsController(ApplicationDbContext context, 
                                  RoleManager<IdentityRole> roleManager, 
                                  UserManager<ApplicationUser> userManager,
                                  IWebHostEnvironment env)
        {
            db = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _env = env;
/*
            // selectam userul curent
            *//*            currentUserId = _userManager.GetUserId(User);
            */
        }

        // afisarea tuturor proiectelor/echipelor al userului curent
        public IActionResult Index()
        {
            // selectam proiectele care au drept user, userul curent
            var projects = db.UserProjects.Include("Project")/*
                             .Where(up => up.UserId == currentUserId)*/;

            // trimitem proiectele selectate catre view
            ViewBag.Projects = projects;

            // verificam daca exista vreun TempData neafisat
            // daca exista, il afisam
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"];

            return View();
        }

        // afisarea unui proiect particular

        // mai trebuie testata:))

        public IActionResult Show(int? id)
        {
            // verificam daca nr de participanti dintr-o echipa a fost modificat
            if (TempData.ContainsKey("members"))
                ViewBag.Message = TempData["members"];

            // selectam proiectul care are drept user, userul curent
            // si are idul transmis ca parametru
            Project project = db.Projects
                                .Where(project => project.Id == (int)id)
                                .First();


            // verificam daca userul care acceseaza proiectul detine acest proiect
            /*if (project.UserId == currentUserId)
            {*/
                // lista de taskuri fara stapan:))
                ICollection<Task> unassignedTasks = new List<Task>();

                // selectam taskurile ramase, neasignate
                var tasks = db.Tasks.Include("Project")
                              .Where(task => task.ProjectId == (int)id && task.UserId == null);

                // adaugam taskul in lista de sarcini neatribuite
                foreach (Task task in tasks)
                    unassignedTasks.Add(task);

                project.Tasks = unassignedTasks;

                // realizam o lista a tuturor membrilor din cadrul acestui proiect
                var members = from user in db.ApplicationUsers join userproject in db.UserProjects on user.Id equals userproject.UserId
                                      where userproject.ProjectId == (int)id
                                      select user;

                // pentru fiecare membru in parte ii selectam taskurile asignate
                foreach(ApplicationUser member in members)
                    member.Tasks = GetAllTasksMember(member.Id, (int)id);
                
                // o trimitem catre view in viewbag-ul members
                ViewBag.Members = members;

                return View(project);
            /*}
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa accesezi acest proiect";
                return RedirectToAction("Index");
            }*/
        }

        // metoda new cu get
        public IActionResult New()
        {
            Project project = new Project();
            // trimitem o lista cu toti userii disponibili, mai putin userul curent
            ViewBag.Users = GetAllUsers();  

            return View(project);
        }

        //metoda new cu post
        
        // projectPhoto = fotografia de coperta a proiectului, personalizata de user
        // Users = lista de useri selectati de organizator ca sa faca parte din echipa curenta
        [HttpPost]
        public async Task<IActionResult> New(Project requestProject, IFormFile? projectPhoto, List<string>? Users)
        {
            if (ModelState.IsValid)
            {
                requestProject.StartDate = DateTime.Now;
                //requestProject.OrganizatorId = currentUserId;

                // userul a incarcat o poza
                if (projectPhoto != null)
                {
                    // setam calea unde va fi salvata fotografia
                    // folderul 'images' din wwwroot
                    var storagePath = Path.Combine(_env.WebRootPath,
                                                        "images",
                                                        projectPhoto.FileName);
                    
                    // setam calea formala, care va fi salvata in baza de date
                    var databaseFile = "/images/" + projectPhoto.FileName;
                    // salvam fotografia
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await projectPhoto.CopyToAsync(fileStream);
                    }

                    requestProject.Photo = databaseFile;
                }
                // daca userul nu a incarcat vreo fotografie
                // proiectului ii va fi atribuita o fotografie default
                else requestProject.Photo = "/images/" + "pexels-pixabay-163811.jpg";

                // adaugam proiectul si salvam modificarile, pentru ca acesta sa primeasca un id in baza de date
                db.Projects.Add(requestProject);
                db.SaveChanges();

                // adaugam legaturile in userprojects, atat pentru userii selectati drept participanti,
                // cat si pentru userul curent, organizatorul echipei

                /*UserProject userproject = new UserProject();
                userproject.UserId = currentUserId;
                userproject.ProjectId = requestProject.Id;
                db.UserProjects.Add(userproject);*/

                foreach (var user in Users)
                {
                    UserProject userproject = new UserProject();
                    userproject.UserId = user;
                    userproject.ProjectId = requestProject.Id;

                    db.UserProjects.Add(userproject);
                }
                db.SaveChanges();

                // informam userul ca modificarea a fost realizata
                TempData["message"] = "Proiectul a fost adaugat. Succes!:)";
                return RedirectToAction("Index");
            }
            else
            {
                // returnam view-ul utilizatorului, pana ce modelul devine valid
                ViewBag.Users = GetAllUsers();
                return View(requestProject);
            }
        }

        // metoda edit cu get
        public IActionResult Edit(int? id)
        {
            // selectam proiectul cu id-ul transmis ca parametru
            Project project = db.Projects.Where(p => p.Id == (int)id).First();
            
            return View(project);
        }
        // metoda edit cu post

        // projectPhoto = fotografia de coperta a proiectului, personalizata de user
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Project requestProject, IFormFile? projectPhoto)
        {
            if (ModelState.IsValid)
            {
                // selectam proiectul modificat
                Project project = db.Projects.Where(p => p.Id == (int)id).First();

                // salvam noile valori ale atributelor in baza de date
                project.Title = requestProject.Title;
                project.Description = requestProject.Description;
                project.Deadline = requestProject.Deadline;
                project.StartDate = DateTime.Now;

                if (projectPhoto != null)
                {
                    // setam calea unde va fi salvata fotografia
                    // folderul 'images' din wwwroot
                    var storagePath = Path.Combine(_env.WebRootPath,
                                                        "images",
                                                        projectPhoto.FileName);

                    // setam calea formala, care va fi salvata in baza de date
                    var databaseFile = "/images/" + projectPhoto.FileName;
                    // salvam fotografia
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await projectPhoto.CopyToAsync(fileStream);
                    }

                    project.Photo = databaseFile;
                }
                // daca userul nu a incarcat vreo fotografie
                // proiectului ii va fi atribuita o fotografie default
                else project.Photo = "/images/" + "pexels-pixabay-163811.jpg";

                db.SaveChanges();

                // informam userul ca modificarea a fost realizata
                TempData["message"] = "Proiectul a fost actualizat cu succes!";
                return RedirectToAction("Index");
            }
            else
            {
                // returnam view-ul utilizatorului, pana ce modelul devine valid
                return View(requestProject);
            }
        }

        //metoda delete
        public IActionResult Delete(int? id)
        {
            // selectam proiectul ce urmeaza a fi sters
            Project project = db.Projects
                                .Where(project => project.Id == (int)id)
                                .First();

            // il stergem
            db.Projects.Remove(project);
            db.SaveChanges();

            // informam userul de succesul actiunilor realizate
            TempData["message"] = "Proiectul a fost sters";
            // il trimitem inapoi in index
            return RedirectToAction("Index");
        }

        // metoda editare echipa cu get
        public IActionResult EditMembers(int? id)
        {
            // selectam toti utilizatorii inregistrati, mai putin userul curent
            var users = GetAllUsers();
            ViewBag.Users = users;

            // selectam userii care figurau drept participanti in cadrul proiectului curent
            var currentUsers = new List<string>();

            var currentUsersDB = db.UserProjects.Include("User")
                                .Where(up => up.ProjectId == (int)id);

            // si ii adaugam intr-o lista
            foreach(UserProject user in currentUsersDB)
                currentUsers.Add(user.UserId);
            
            ViewBag.currentUsers = currentUsers;

            return View();
        }
        [HttpPost]
        public IActionResult EditMembers(int? id, List<string> newUsers)
        {
            // selectam userii care figurau drept participanti in cadrul proiectului curent
            var oldUsers = new List<string>();

            var oldUsersDB = db.UserProjects.Include("User")
                                .Where(up => up.ProjectId == (int)id);

            // si ii adaugam intr-o lista
            foreach (UserProject user in oldUsersDB)
                oldUsers.Add(user.UserId);

            // verificam daca userii vechi au ramas in echipa
            // si ii adaugam pe cei noi
            foreach (string user in oldUsers.Union(newUsers))

                // daca userul selectat nu facea parte din echipa, il adaugam
                if (!oldUsers.Contains(user))
                {
                    UserProject userproject = new UserProject();
                    userproject.UserId = user;
                    userproject.ProjectId = (int)id;

                    db.UserProjects.Add(userproject);
                }
                // daca userul selectat facea parte din echipa, dar acum nu o mai face, il eliminam
                else if (!newUsers.Contains(user)){

                    // selectam legatura dintre userul respectiv si proiectul curent
                    // din tabela userprojects
                    var userproject = db.UserProjects.Where(up => up.ProjectId == (int)id && up.UserId == user).First();
                    // si o eliminam
                    db.UserProjects.Remove(userproject);
                    // stim ca este o singura legatura, deoarece adaugarea se realizeaza
                    // doar daca userul nou nu se afla printre cei vechi => o singura instanta
               }
            
            db.SaveChanges();
            // informam utilizatorul de succesul modifiacrilor facute
            TempData["members"] = "Echipa acestui proiect a fost modificata!";

            // il redirectionam catre view-ul show al proiectului curent
            return Redirect("~/Projects/Show/" + id);
        }

        [NonAction]
        public ICollection<Task> GetAllTasksMember(string id, int projectid)
        {
            ICollection<Task> memberTasks = new List<Task>();

            // selectam taskurile proiectului projectid, asignate userului id
            var tasks = db.Tasks.Include("User")
                          .Where(task => task.ProjectId == projectid && task.UserId == id);

            // si le adaugam in colectie
            foreach(Task task in tasks)
                memberTasks.Add(task);

            return memberTasks;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllUsers()
        {
            var Users = new List<SelectListItem>();

            // selectam toti userii inregistrati penru a-i adauga, eventual, drept membrii in proiectul actual
            var users = db.ApplicationUsers/*.Where(user => user.Id != currentUserId)*/;
            
            // cream colectia ce contine tupluri (Text, Valoare)
            foreach (ApplicationUser user in users)
            {
                Users.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.Email.ToString()
                });
            }
            return Users;
        }
    }
}
