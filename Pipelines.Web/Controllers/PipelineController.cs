using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pipelines.Web.Models;
using Pipelines.Data.Context;
using Pipelines.Data.Models;
using Microsoft.AspNet.SignalR;
using Pipelines.Web.Hubs;
using Pipelines.Web.Helpers;



namespace Pipelines.Web.Controllers
{
    public class PipelineController : Controller
    {

        public void Add(FormCollection form)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var pipeline = new Pipeline(name: form["title"],
                                            boardId: int.Parse(form["board"]));

                ctx.Boards.Attach(pipeline.Board);
                ctx.Pipelines.Add(pipeline);
                ctx.SaveChanges();

                if (ctx.Stages.Where(x => x.Board.BoardId == pipeline.Board.BoardId).Count() > 0)
                {
                    GlobalHost.ConnectionManager.GetHubContext<BoardHub>().Clients.Group(String.Format("Pipelines .::. {0}", pipeline.Board.Name)).renderPipeline(pipeline.PipelineId, form["title"], "false", new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(ctx.Stages.Where(x => x.Board.BoardId == pipeline.Board.BoardId).Select(x => x.StageId).ToArray()));
                }
                else
                {
                    GlobalHost.ConnectionManager.GetHubContext<BoardHub>().Clients.Group(String.Format("Pipelines .::. {0}", pipeline.Board.Name)).updateStatus("You have pending pipelines but no stages, to get started add a stage.");
                }
            }
        
        }
        public void Subscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.PipelineSubscriptions.FirstOrDefault(x => x.Pipeline.PipelineId == id && x.User.Username == HttpContext.User.Identity.Name) == null)
                {
                    var subscription = new PipelineSubscription();
                    subscription.Pipeline = ctx.Pipelines.Where(p => p.PipelineId == id).First();
                    subscription.User = ctx.Users.First(u => u.Username == HttpContext.User.Identity.Name);

                    ctx.PipelineSubscriptions.Add(subscription);
                    ctx.SaveChanges();
                }
            }
        }

        public void Unsubscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.PipelineSubscriptions.Where(x => x.Pipeline.PipelineId == id && x.User.Username == HttpContext.User.Identity.Name).FirstOrDefault() != null)
                {
                    var subscription = ctx.PipelineSubscriptions.Where(p => p.Pipeline.PipelineId == id && p.User.Username == HttpContext.User.Identity.Name).First();

                    ctx.PipelineSubscriptions.Remove(subscription);
                    ctx.SaveChanges();
                }
            }
        }

    }
}
