using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pipelines.Data.Context;
using System.Linq;

namespace Pipelines.Data.Models
{
    [Table("tb_Card")]
    public class Card
    {
        [Key]
        public int CardId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Contents { get; set; }

        [Required]
        public bool Visible { get; set; }

        public virtual Board Board { get; set; }
        public virtual Pipeline Pipeline { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual User User { get; set; }

        public Card()
        { 
        
        }

        public Card(string title, string description, string contents,int boardId, int pipelineId, int stageId, string username)
        { 
            this.Title = title;
            this.Description = description;
            this.Contents = contents;
            this.Visible = true;

            using (var meh = new PipelinesDBContext())
            {
                this.Board = meh.Boards.Where(x => x.BoardId == boardId).First();
                this.Pipeline = meh.Pipelines.Where(x => x.PipelineId == pipelineId).First();
                this.Stage = meh.Stages.Where(x => x.StageId == stageId).First();
                this.User = meh.Users.Where(x => x.Username == username).First();
            }
        }

    }
}
