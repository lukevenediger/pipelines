using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pipelines.Data.Context;
using System.Linq;

namespace Pipelines.Data.Models
{
    [Table("tb_Stage")]
    public class Stage
    {
        [Key]
        public int StageId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual Board Board { get; set; }

        public Stage()
        { 
        
        }

        public Stage(string name, int boardId)
        {
            this.Name = name;

            using (var ctx = new PipelinesDBContext())
            {
                this.Board = ctx.Boards.First(x => x.BoardId == boardId);
            }
        }
    }
}
