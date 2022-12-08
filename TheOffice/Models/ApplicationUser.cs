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

        [Required(ErrorMessage ="Username field is required")]
        public override string UserName { get; set; }

        [Required(ErrorMessage ="Birthday field is rquired")]
        public DateTime Birthday;

        public string? ProfilePhoto { get; set; }
        public string? Description { get; set; }

        // legatura cu tabelul echipe
        public virtual ICollection<UserTeam>? UserTeams { get; set; }

        public virtual ICollection<Team>? Teams { get; set; }
        public virtual ICollection<TheOffice.Models.Task>? Tasks { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
