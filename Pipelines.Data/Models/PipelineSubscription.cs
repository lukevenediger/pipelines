using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pipelines.Data.Models
{
    [Table("tb_PipelineSubscription")]
    public class PipelineSubscription
    {
        [Key]
        public int PipelineSubscriptionId { get; set; }

        public virtual User User { get; set; }
        public virtual Pipeline Pipeline { get; set; }
    }
}
