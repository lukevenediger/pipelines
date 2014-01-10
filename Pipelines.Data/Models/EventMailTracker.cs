using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pipelines.Data.Models
{
    [Table("tb_EventMailTracker")]
    public class EventMailTracker
    {
        [Key]
        public int TrackerId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual CardEvent CardEvent { get; set; }
    }
}
