using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Pipelines.Web.Models;
using System.Linq;
using Pipelines.Data.Context;
using Pipelines.Data.Models;
using Pipelines.Web.Structures;

namespace Pipelines.Web.Hubs
{
    public class BoardHub : Hub
    {
        public void Join(string Room)
        {
             Groups.Add(Context.ConnectionId, Room);
        }
        public void LockCard(int TaskID, string Room)
        {
            Clients.Group(Room, new string[] { Context.ConnectionId }).lockCard(TaskID);
        }
        public void UnlockCard(int TaskId, string Room)
        {
            Clients.Group(Room, new string[] { Context.ConnectionId }).unlockCard(TaskId);
        }
        public void Move(int TaskID, int StageID, int PipelineID, string Room, string username)
        { 
            using (var ctx = new PipelinesDBContext())
            {
               var card = ctx.Cards.First(c => c.CardId == TaskID);
               card.User = ctx.Users.First(u => u.Username == username);
               
               //var stageInitialTime = ctx.CardEvents.Where(x => x.Card.CardId == card.CardId && x.PastStage.StageId != x.Stage.StageId && x.Stage.StageId == card.Stage.StageId).OrderByDescending(x => x.Timestamp).First().Timestamp;
               //var delayInMinutes = (DateTime.Now - stageInitialTime).Minutes;
               
               card.Stage = ctx.Stages.First(x => x.StageId == StageID);
               card.Pipeline = ctx.Pipelines.First(x => x.PipelineId == PipelineID); 
               ctx.SaveChanges();
                
               //var newStageName = card.Stage.Name;
               //var delayInMinutes = (DateTime.Now - stageInitialTime).Minutes;
               //Metrics.Metrics.LogCount(string.Format( "{0}.{1}.{2}.DelayInMinutes", Room, oldStageName, card.Pipeline.Name ), delayInMinutes );
            };

            Clients.Group(Room).MoveCard(TaskID, StageID, PipelineID);

        }


        public void RenderStage(int stageId, string name, string room)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var board = ctx.Stages.First(x => x.StageId == stageId).Board.BoardId;
                if (ctx.Pipelines.Where(x => x.Board.BoardId == board).Count() > 0)
                {
                    Clients.Group(room).renderStage(stageId, name);
                }
                else
                {
                    Clients.Group(room).updateStatus("You have pending stages but no pipelines, to get started add a pipeline.");
                }
            }

        }
        public void RemoveCard(int cardId, string room)
        {
            Clients.Group(room).removeCard(cardId);
        }
    }
}