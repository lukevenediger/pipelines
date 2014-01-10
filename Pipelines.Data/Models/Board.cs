using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pipelines.Data.Models
{
    [Table("tb_Board")]
    public class Board
    {
        [Key]
        public int BoardId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual User User { get; set; }
    }
}
