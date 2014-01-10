using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Pipelines.Web.Models;
using Pipelines.Data.Context;
using Pipelines.Data.Models;
using Microsoft.AspNet.SignalR;
using Pipelines.Web.Hubs;
using Pipelines.Web.Helpers;
using Pipelines.Web.Structures;
using System.Data.Entity;

namespace Pipelines.Web.Controllers
{
    public class CardController : Controller
    {
        public void Add(FormCollection form)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var card = new Card(title: form["title"],
                                    description: form["description"],
                                    contents: "Test",
                                    boardId: int.Parse(form["board"]),
                                    pipelineId: int.Parse(form["pipeline"]),
                                    stageId: int.Parse(form["stage"]),
                                    username: HttpContext.User.Identity.Name);

                ctx.Boards.Attach(card.Board);
                ctx.Pipelines.Attach(card.Pipeline);
                ctx.Stages.Attach(card.Stage);
               
                ctx.Cards.Add(card);
                ctx.SaveChanges();

   

                GlobalHost.ConnectionManager.GetHubContext<BoardHub>().Clients.All.renderCard(card.CardId, int.Parse(form["pipeline"]),int.Parse(form["stage"]), form["title"], form["description"], false);
            }
        }
        public void Subscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.CardSubscriptions.FirstOrDefault(x => x.Card.CardId == id && x.User.Username == HttpContext.User.Identity.Name) == null)
                {
                    var subscription = new CardSubscription
                    {
                        User = ctx.Users.First(u => u.Username == HttpContext.User.Identity.Name),
                        Card = ctx.Cards.First(x => x.CardId == id)
                    };

                    ctx.CardSubscriptions.Add(subscription);
                    ctx.SaveChanges();

                   
                }

            }
        }
        public void Unsubscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.CardSubscriptions.FirstOrDefault(x => x.Card.CardId == id && x.User.Username == HttpContext.User.Identity.Name) != null)
                {
                    var subscription = ctx.CardSubscriptions.Include(x => x.Card).First(x => x.Card.CardId == id && x.User.Username == HttpContext.User.Identity.Name);

                    ctx.CardSubscriptions.Remove(subscription);
                    ctx.SaveChanges();
                }
            }
        }
        public JsonResult UpdateCardTitle(int pk, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Json(new { status = "error", message = "The title cannot be empty." });
            }

            using (var ctx = new PipelinesDBContext())
            {
                var card = ctx.Cards.FirstOrDefault(x => x.CardId == pk);
                
                if (card == null)
                {
                    return Json(new { status = "error", message = "The card was not found." });
                }

                card.Title = value;
                ctx.SaveChanges();

                GlobalHost.ConnectionManager.GetHubContext<BoardHub>().Clients.All.renderCard(card.CardId, card.Pipeline.PipelineId, card.Stage.StageId, card.Title, card.Description);
            }

            return Json(new { status = "success", message = "The card was updated successfully." });
        }
        public JsonResult UpdateCardDescription(int pk, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Json(new { status = "error", message = "The description cannot be empty." });
            }

            using (var ctx = new PipelinesDBContext())
            {
                var card = ctx.Cards.FirstOrDefault(x => x.CardId == pk);
                
                if (card == null)
                {
                    return Json(new { status = "error", message = "The card was not found." });
                }

                card.Description = value;
                ctx.SaveChanges();
                
                GlobalHost.ConnectionManager.GetHubContext<BoardHub>().Clients.All.renderCard(card.CardId, card.Pipeline.PipelineId , card.Stage.StageId , card.Title, card.Description);
            }

            return Json(new { status = "success", message = "The card was updated successfully." });
        }


        public JsonResult AddCard(Card card)
        {
            using (var ctx = new PipelinesDBContext())
            {
                try
                {
                    ctx.Cards.Add(card);
                    ctx.SaveChanges();
                }
                catch (InvalidOperationException)
                {
                    return Json(new { status = "fail" });
                }

                return Json(new { status = "success" });
            }

            
        }
        
        public JsonResult Remove(int id)
        { 
            using(var ctx = new PipelinesDBContext())
            {
                var card = ctx.Cards.FirstOrDefault(x => x.CardId == id);

                if (card != null)
                {
                    try
                    {
                        card.Visible = false;
                        ctx.SaveChanges();

                        return Json(new { status = "success", message = "Card removed." });
                    }
                    catch (InvalidOperationException)
                    {
                        return Json(new { status = "fail", message = "Something went wrong while trying to remove the card." });
                    }                 
                }

                return Json(new { status = "fail", message = "Card not found." });
            }
        }
    }
}