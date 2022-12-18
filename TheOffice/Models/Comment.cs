using System.ComponentModel.DataAnnotations;

namespace TheOffice.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
        [MinLength(5, ErrorMessage = "Continutul trebuie sa aiba cel putin de 5 caractere")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int Likes { get; set; }
        public int Dislikes { get; set; }

        public int? TaskId { get; set; }

        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Task? Task { get; set; }
    }

}

