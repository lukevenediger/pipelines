using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pipelines.Data.Context;
using System.Linq;

namespace Pipelines.Data.Models
{
    [Table("tb_Pipeline")]
    public class Pipeline
    {
        public Pipeline ()
        {
    
        }

        public Pipeline(string name, int boardId)
        {
            using (var ctx = new PipelinesDBContext())
            {
                this.Name = name;
                this.Board = ctx.Boards.First(x => x.BoardId == boardId);
            }
        }

        [Key]
        public int PipelineId { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual Board Board { get; set; }
    }
}
