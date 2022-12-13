using System.ComponentModel.DataAnnotations;

namespace TheOffice.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea proiectului este obligatorie")]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime Deadline { get; set; }

        public string? OrganizatorId { get; set; }

        public string? Photo { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }

        // legatura cu tabelul user
        public virtual ICollection<UserProject>? UserProjects { get; set; }
    }
}

