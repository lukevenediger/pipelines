using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pipelines.Data.Models;

namespace Pipelines.Data.Context
{
    public class PipelinesDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardEvent> CardEvents { get; set; }
        public DbSet<CardSubscription> CardSubscriptions { get; set; }
        public DbSet<EventMailTracker> EventMailTrackers { get; set; }
        public DbSet<Pipeline> Pipelines { get; set; }
        public DbSet<PipelineSubscription> PipelineSubscriptions { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<StageSubscription> StageSubscriptions { get; set; }
    }
}
