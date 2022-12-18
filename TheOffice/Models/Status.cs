using System.ComponentModel.DataAnnotations;

namespace TheOffice.Models
{
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public string Status_Value { get; set; }

        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
