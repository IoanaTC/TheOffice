using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheOffice.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage ="Last Name field is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage ="First Name field is required")]
        public string LastName { get; set; }

        // username ales de utilizator
        [Required(ErrorMessage ="Username field is required")]
        public override string UserName { get; set; }

        [Required(ErrorMessage ="Birthday field is rquired")]
        public DateTime Birthday;

        public string? ProfilePhoto { get; set; }
        public string? Description { get; set; }

        // legatura cu tabelul proiecte
        public virtual ICollection<UserProject>? UserProjects { get; set; }

        // liste de taskuri si comentarii ale fiecarui user
        public virtual ICollection<Task>? Tasks { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }

        // lista cu roluri, adminul poate asigna/revoca roluri
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
