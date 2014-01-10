using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Pipelines.Data.Models
{
    [Table("tb_CardEventHistory")]
    public class CardEvent
    {
        [Key]
        public int EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual Card Card { get; set; }
        public virtual Board PastBoard { get; set; }
        public virtual Board Board { get; set; }
        public virtual Pipeline PastPipeline { get; set; }
        public virtual Pipeline Pipeline { get; set; }
        public virtual Stage PastStage { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual User User { get; set; } 
      }
}
