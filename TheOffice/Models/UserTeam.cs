using System.ComponentModel.DataAnnotations.Schema;

namespace TheOffice.Models
{
    public class UserTeam
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public int? TeamId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Team? Team { get; set; }

        public DateTime? UserAddedDate { get; set; }
        public string? Badge { get; set; }
    }
}
