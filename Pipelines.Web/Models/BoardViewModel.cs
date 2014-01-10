using System.Collections.Generic;
using Pipelines.Data.Models;

namespace Pipelines.Web.Models
{
    public class BoardViewModel
    {
        public BoardViewModel(int boardId, List<Stage> stages, List<Pipeline> pipelines, List<Card> cards, List<CardSubscription> cardsubscriptions, List<PipelineSubscription> pipelinesubscriptions, List<StageSubscription> stagesubscriptions)
        {
            this.BoardId = boardId;
            this.Stages = stages;
            this.Pipelines = pipelines;
            this.Cards = cards;
            this.CardSubscriptions = cardsubscriptions;
            this.PipelineSubscriptions = pipelinesubscriptions;
            this.StageSubscriptions = stagesubscriptions;
        }


        public int BoardId { get; set; }
        public List<Stage> Stages { get; set; }
        public List<Pipeline> Pipelines { get; set; }
        public List<Card> Cards { get; set; }
        public List<CardSubscription> CardSubscriptions { get; set; }
        public List<StageSubscription> StageSubscriptions { get; set; }
        public List<PipelineSubscription> PipelineSubscriptions { get; set; }
    }
}