using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheOffice.Data;
using TheOffice.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using System.Data;

using Task = TheOffice.Models.Task;
using Project = TheOffice.Models.Project;
/*using Humanizer;
*/using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Principal;

namespace TheOffice.Controllers
{

    [Authorize]
    public class ProjectsController : Controller
    {
        // conexiunea cu baza de date
        private readonly ApplicationDbContext db;

        // managerul de useri si roluri
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // manager de fisiere
        private IWebHostEnvironment _env;

        // userul curent
        private string currentUserId;


        public ProjectsController(ApplicationDbContext context, 
                                  RoleManager<IdentityRole> roleManager, 
                                  UserManager<ApplicationUser> userManager,
                                  IWebHostEnvironment env,
                                  SignInManager<ApplicationUser> signInManager)
        {
            db = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }


        // afisarea tuturor proiectelor/echipelor al userului curent
        public IActionResult Index()
        {
            // modfic view-ul a.i. fiecare utilizator sa vada doar butoanele permise in functie de rol
            SetAccesRights(null);

            // afisarea tuturor proiectelor pe care userul curent are dreptul sa le vada

            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            var projects = new List<Project>();

            if (User.IsInRole("Admin"))
            {
                // userul este administrator => vede toate proiectele 
                var adminProjects = db.Projects;

                foreach(var adminProject in adminProjects)
                    projects.Add(adminProject);
            }
            else
            {
                // selectam proiectele care au drept user, userul curent
                var userProjects = db.UserProjects.Include("Project")
                                     .Where(up => up.UserId == currentUserId);

                
                // userul nu este administrator => vede doar proiectele din care face parte
                foreach (var userproject in userProjects)
                {
                    // selectam proiectul din care face parte userul curent                    {
                    userproject.Project = db.Projects.Where(p => p.Id == userproject.ProjectId).First();
                    projects.Add(userproject.Project);
                }
            }

            // verificam daca exista vreun TempData neafisat
            // daca exista, il afisam
            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"].ToString();

            // Afisare paginata
            int _perPage = 6;

            // Fiind un numar variabil de proiecte, verificam de fiecare data utilizand
            // metoda Count()
            int totalItems = projects.Count();

            // Se preia pagina curenta din View-ul asociat
            // Numarul paginii este valoarea parametrului page din ruta
            // /Projects/Index?page=valoare
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            // Pentru prima pagina offsetul o sa fie zero
            // Pentru pagina 2 o sa fie 3
            // Asadar offsetul este egal cu numarul de proiecte care au fost deja afisate pe paginile anterioare
            var offset = 0;

            // Se calculeaza offsetul in functie de numarul paginii la care suntem
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            // Se preiau proiectele corespunzatoare pentru fiecare pagina la care ne aflam
            // in functie de offset
            var paginatedProjects = projects.Skip(offset).Take(_perPage);

            // Preluam numarul ultimei pagini
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
            ViewBag.Projects = paginatedProjects;
            return View();
        }


        // afisarea unui proiect particular

        // mai trebuie testata:))
        public IActionResult Show(int? id)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            // verificam daca exista proiectul
            var Project = db.Projects.Where(p => p.Id == (int)id);
            // proiectul nu exista
            if(Project == null)
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa accesezi acest proiect sau nu exista";
                return RedirectToAction("Index");
            }

            // selectam legaturile dintre userul curent si proiectul solicitat
            // pt a verifica daca userul care acceseaza proiectul apartine echipei
            var userprojects = db.UserProjects.Where(up => up.ProjectId == (int)id && up.UserId == currentUserId);

            // daca apartine echipei sau daca este admin
            if (userprojects.Count() != 0 || User.IsInRole("Admin"))
            {
                // selectam acest proiect
                Project project = db.Projects
                                .Where(project => project.Id == (int)id)
                                .First();

                // realizam o lista a tuturor membrilor din cadrul acestui proiect
                var members = from user in db.ApplicationUsers
                              join userproject in db.UserProjects on user.Id equals userproject.UserId
                              where userproject.ProjectId == (int)id
                              select user;

                // pentru fiecare membru in parte ii selectam taskurile asignate
                foreach (ApplicationUser member in members)
                    member.Tasks = GetTasks((int)id, member.Id);

                // o trimitem catre view in viewbag-ul members
                ViewBag.Members = members;

                // trimitem catre view si taskurile neatribuite
                ViewBag.Tasks = GetTasks((int)id, null);

                // verificam daca nr de participanti dintr-o echipa a fost modificat
                if (TempData.ContainsKey("members"))
                    ViewBag.Message = TempData["members"];

                if (TempData.ContainsKey("message"))
                    ViewBag.Message_task = TempData["message"].ToString();

                // modfic view-ul a.i fiecare utilizator sa vada doar butoanele permise in functie de rol
                SetAccesRights(project.OrganizatorId);
                return View(project);
            }
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa accesezi acest proiect sau nu exista";
                return RedirectToAction("Index");
            }
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
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // setam data curenta si oranizatorul echipei
                requestProject.StartDate = DateTime.Now;
                requestProject.OrganizatorId = currentUserId;

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

                UserProject userproject = new UserProject();
                userproject.UserId = currentUserId;
                userproject.ProjectId = requestProject.Id;

                db.UserProjects.Add(userproject);

                foreach (var user in Users)
                {
                    UserProject userProject = new UserProject();
                    userProject.UserId = user;
                    userProject.ProjectId = requestProject.Id;

                    db.UserProjects.Add(userProject);
                }
                db.SaveChanges();


                // informam userul ca modificarea a fost realizata
                TempData["message"] = "Proiectul a fost adaugat";


                // userul curent primeste rol de Organizator
                ApplicationUser User = await _userManager.FindByIdAsync(currentUserId);
                await _userManager.AddToRoleAsync(User, "Organizator");
                await _signInManager.SignInAsync(User, isPersistent: false);
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
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult Edit(int? id)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            // selectam proiectul cu id-ul transmis ca parametru
            Project project = db.Projects.Where(p => p.Id == (int)id).First();

            // poate edita doar organizatorul echipei sau adminul
            if (project.OrganizatorId == currentUserId || User.IsInRole("Admin"))
                return View(project);
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa modifici acest proiect";
                return RedirectToAction("Index");
            }
        }

        // metoda edit cu post
        // projectPhoto = fotografia de coperta a proiectului, personalizata de user
        [HttpPost]
        [Authorize(Roles = "Organizator,Admin")]
        public async Task<IActionResult> Edit(int? id, Project requestProject, IFormFile? projectPhoto)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // selectam proiectul modificat
                Project project = db.Projects.Where(p => p.Id == (int)id).First();

                // poate edita doar organizatorul echipei sau adminul
                if (project.OrganizatorId == currentUserId || User.IsInRole("Admin"))
                {
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
                    // trimitem un TempData care sa informeze userul ca accesul este interzis
                    TempData["message"] = "Scuze, nu ai voie sa modifici acest proiect";
                    return RedirectToAction("Index");
                }             
            }
            else
            {
                // returnam view-ul utilizatorului, pana ce modelul devine valid
                return View(requestProject);
            }
        }


        //metoda delete
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult Delete(int? id)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            // selectam proiectul ce urmeaza a fi sters
            Project project = db.Projects.Include("Tasks")
                                          .Where(project => project.Id == (int)id)
                                          .First();

            // doar adminul si organizatorul echipei pot sterge un proiect
            if (project.OrganizatorId == currentUserId || User.IsInRole("Admin"))
            {
                // il stergem
                db.Projects.Remove(project);
                db.SaveChanges();

                // informam userul de succesul actiunilor realizate
                TempData["message"] = "Proiectul a fost sters";
                // il trimitem inapoi in index
                return RedirectToAction("Index");
            }
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa stergi acest proiect";
                return RedirectToAction("Index");
            }
        }



        // metoda editare echipa cu get
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult EditMembers(int? id)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            // selectam proiectul ce urmeaza a fi sters
            Project project = db.Projects
                                .Where(project => project.Id == (int)id)
                                .First();

            // doar adminul sau organizatorul echipei are voie sa modifice echipa unui proiect
            if (project.OrganizatorId == currentUserId || User.IsInRole("Admin"))
            {
                // selectam toti utilizatorii inregistrati, mai putin userul curent
                var users = GetAllUsers();
                ViewBag.Users = users;

                // selectam userii care figurau drept participanti in cadrul proiectului curent
                var currentUsers = new List<string>();

                var currentUsersDB = db.UserProjects.Include("User")
                                    .Where(up => up.ProjectId == (int)id);

                // si ii adaugam intr-o lista
                foreach (UserProject user in currentUsersDB)
                    currentUsers.Add(user.UserId);

                ViewBag.currentUsers = currentUsers;

                return View();
            }
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa modifici echipa acestui proiect";
                return RedirectToAction("Index");
            }
        }
        
        [HttpPost]
        [Authorize(Roles = "Organizator,Admin")]
        public IActionResult EditMembers(int? id, List<string> newUsers)
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            // selectam proiectul ce urmeaza a fi sters
            Project project = db.Projects
                                .Where(project => project.Id == (int)id)
                                .First();

            // doar adminul sau organizatorul echipei are voie sa modifice echipa unui proiect
            if (project.OrganizatorId == currentUserId || User.IsInRole("Admin"))
            {
                // selectam userii care figurau drept participanti in cadrul proiectului curent
                var oldUsers = new List<string>();

                var oldUsersDB = db.UserProjects.Include("User")
                                    .Where(up => up.ProjectId == (int)id);

                // si ii adaugam intr-o lista
                foreach (UserProject user in oldUsersDB)
                    if (user.UserId != currentUserId)
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
                    else if (!newUsers.Contains(user))
                    {

                        // selectam legatura dintre userul respectiv si proiectul curent
                        // din tabela userprojects
                        var userproject = db.UserProjects.Where(up => up.ProjectId == (int)id && up.UserId == user).First();
                        // si o eliminam
                        db.UserProjects.Remove(userproject);
                        // stim ca este o singura legatura, deoarece adaugarea se realizeaza
                        // doar daca userul nou nu se afla printre cei vechi => o singura instanta

                        // eliberam taskurile atribuite userului sters
                        // selectam taskurile proiectului projectid, asignate userului id
                        var tasks = db.Tasks.Include("User")
                                      .Where(task => task.ProjectId == id && task.UserId == user);
                        foreach (Task task in tasks)
                            task.UserId = null;
                    }

                db.SaveChanges();
                // informam utilizatorul de succesul modifiacrilor facute
                TempData["members"] = "Echipa acestui proiect a fost modificata!";

                // il redirectionam catre view-ul show al proiectului curent
                return Redirect("~/Projects/Show/" + id);
            }
            else
            {
                // trimitem un TempData care sa informeze userul ca accesul este interzis
                TempData["message"] = "Scuze, nu ai voie sa modifici echipaui acest proiect";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public JsonResult AssignTask(int itemId, string parentId)
        {
            Console.WriteLine(itemId+" "+parentId);

            var parent = db.Users.Where(user => user.Id == parentId);
            if (parent != null)
            {
                var task = db.Tasks.Include("User")
                         .Where(task => task.Id == itemId).First();

                task.UserId = parentId;

                db.SaveChanges();
            }

            return Json(new { message = "Succes" });
        }
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> admin()
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);
            var users = db.Users.Where(user => user.Id != currentUserId).ToList();

            ViewBag.Users = users;

            for(var i=0; i<users.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(users[i].Id);
                var roles = await _userManager.GetRolesAsync(user);

                Console.WriteLine(roles);
                ViewBag.Roles[i] = roles;
            }
            return View();
        }

        /*[HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> admin(List<string> newUsers)
        {

        }*/

        [NonAction]
        public ICollection<Task> GetTasks(int projectid, string? userid)
        {
            ICollection<Task> Tasks = new List<Task>();

            // selectam taskurile proiectului projectid, asignate userului id
            var tasks = db.Tasks.Include("User")
                          .Where(task => task.ProjectId == projectid && task.UserId == userid);

            // si le adaugam in colectie
            foreach(Task task in tasks)
                Tasks.Add(task);

            return Tasks;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllUsers()
        {
            // selectam userul curent
            currentUserId = _userManager.GetUserId(User);

            var Users = new List<SelectListItem>();

            // selectam toti userii inregistrati penru a-i adauga, eventual, drept membrii in proiectul actual
            var users = db.ApplicationUsers.Where(user => user.Id != currentUserId);
            
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

        // permisiunile utilizatorilor in functie de roluri
        // adminul poate vedea, adauga si edita orice proiect
        // organizatorul poate vedea proiectele din care face parte, edita proiectele create de acesta, adauga 
        // userul poate vedea proiectele din acre face parte, adauga => devine organizator
        [NonAction]
        public void SetAccesRights(string? projectUserId)
        {
            // utilizatorul este admin
            ViewBag.isAdmin = User.IsInRole("Admin");

            // utilizatorul este organizator, iar proiectul curent ii apartine
            if (projectUserId == currentUserId)
                ViewBag.isOrganizator = true;
            else ViewBag.isOrganizator = false;
        }
    }
}
