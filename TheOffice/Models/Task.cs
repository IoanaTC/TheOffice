using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheOffice.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea task-ului este obligatorie")]
        public string Content { get; set; }

        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Deadline-ul task-ului este obligatoriu")]
        public DateTime? Deadline { get; set; }

        public int? StatusId { get; set; }
        public string? Attachment { get; set; }

        public int? ProjectId { get; set; }
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Project? Project { get; set; }

        public virtual Status? Status { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Stat { get; set; }

    }
}