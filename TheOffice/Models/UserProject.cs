using System.ComponentModel.DataAnnotations.Schema;

namespace TheOffice.Models
{
    public class UserProject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public int? ProjectId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Project? Project { get; set; }

        public DateTime UserAddedDate { get; set; }
    }
}
