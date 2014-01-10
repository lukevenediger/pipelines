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
    public class StageController : Controller
    {
        [HttpPost]
        public JsonResult Add(FormCollection form)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var stage = new Stage(name: form["title"],
                                      boardId: int.Parse(form["board"]));

                try
                {
                    ctx.Boards.Attach(stage.Board);
                    ctx.Stages.Add(stage);
                    ctx.SaveChanges();

                    return Json(new { status = "success", stageId = stage.StageId, name = stage.Name});
                }
                catch (InvalidOperationException)
                {
                    return Json(new { status = "fail", message = "Something went wrong. Please contact support." });
                }
            }
        }
        public void Subscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.StageSubscriptions.FirstOrDefault(x => x.Stage.StageId == id && x.User.Username == HttpContext.User.Identity.Name) == null)
                {
                    var subscription = new StageSubscription
                    {
                        User = ctx.Users.First(u => u.Username == HttpContext.User.Identity.Name),
                        Stage = ctx.Stages.First(s => s.StageId == id)
                    };

                    ctx.StageSubscriptions.Add(subscription);
                    ctx.SaveChanges();
                }
            }
        }
        public void Unsubscribe(int id)
        {
            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.StageSubscriptions.Where(x => x.Stage.StageId == id && x.User.Username == HttpContext.User.Identity.Name).FirstOrDefault() != null)
                {
                    var subscription = ctx.StageSubscriptions.First(x => x.Stage.StageId == id && x.User.Username == HttpContext.User.Identity.Name);

                    ctx.StageSubscriptions.Remove(subscription);
                    ctx.SaveChanges();
                }
            }
        
        }
    }
}
