using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pipelines.Data.Models
{
    [Table("tb_CardSubscription")]
    public class CardSubscription
    {
        [Key]
        public int CardSubscriptionId { get; set; }

        public virtual User User { get; set; }
        public virtual Card Card { get; set; }

        public CardSubscription()
        { 
        }

        public CardSubscription(User user, Card card)
        {
            this.User = user;
            this.Card = card;
        }
    }
}
