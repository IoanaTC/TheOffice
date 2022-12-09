using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheOffice.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage ="Prenumele este obligatoriu")]
        public string FirstName { get; set; }

        [Required(ErrorMessage ="Numele este obligatoriu")]
        public string LastName { get; set; }

        // username ales de utilizator
        [Required(ErrorMessage ="Trebuie sa introduceti un username")]
        public override string UserName { get; set; }

        [Required(ErrorMessage ="Trebuie sa introduceti data nasterii dvs")]
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
