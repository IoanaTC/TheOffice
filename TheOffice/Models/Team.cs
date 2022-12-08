using System.ComponentModel.DataAnnotations;

namespace TheOffice.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Team Name is required")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        // legatura cu tabelul user
        public virtual ICollection<UserTeam>? UserTeams { get; set; }

        public ICollection<Project>? Projects { get; set; }

    }
}
