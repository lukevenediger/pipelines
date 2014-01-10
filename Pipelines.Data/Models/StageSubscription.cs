using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pipelines.Data.Models
{
    [Table("tb_StageSubscription")]
    public class StageSubscription
    {
        [Key]
        public int StageSubscriptionId { get; set; }

        public virtual User User { get; set; }
        public virtual Stage Stage { get; set; }
    }
}
