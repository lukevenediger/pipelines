using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pipelines.Web.Models;
using Pipelines.Data.Context;
using Pipelines.Data.Models;
using System;


namespace Pipelines.Web.Controllers
{
    public class BoardController : Controller
    {
        public ActionResult Index(string Name)
        {
            ViewBag.Title = "Pipelines .::. " + Name;

            //TODO: Add user support, we use to use AD here 
            ViewData["email"] = "taygibb@gmail.com";
            ViewData["name"] = "Taylor Gibb";


            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Username == HttpContext.User.Identity.Name) == null)
                {
                    ctx.Users.Add(new User() { Username = HttpContext.User.Identity.Name, Email = ViewData["email"].ToString() });
                    ctx.SaveChanges();
                }

                var board = ctx.Boards.FirstOrDefault(i => i.Name == Name);
                if (board != null)
                {
                    var pipelines = ctx.Pipelines.Where(p => p.Board.BoardId == board.BoardId).ToList();
                    var stages = ctx.Stages.Where(s => s.Board.BoardId == board.BoardId).ToList();
                    var cards = ctx.Cards.Where(c => c.Board.BoardId == board.BoardId && c.Visible == true).ToList();

                    var userid = ctx.Users.First(u => u.Username == HttpContext.User.Identity.Name).UserId;
                    var stagesubscriptions = ctx.StageSubscriptions.Where(x => x.User.UserId == userid && x.Stage.Board.BoardId == board.BoardId).ToList();
                    var pipelinesubscriptions = ctx.PipelineSubscriptions.Where(x => x.User.UserId == userid && x.Pipeline.Board.BoardId == board.BoardId).ToList();
                    var cardsubscriptions = ctx.CardSubscriptions.Where(x => x.User.UserId == userid && x.Card.Visible == true && x.Card.Board.BoardId == board.BoardId).ToList();

                    return View(new BoardViewModel(board.BoardId, stages, pipelines, cards, cardsubscriptions, pipelinesubscriptions, stagesubscriptions));
                }
                else
                {
                    return new HttpNotFoundResult("Board Not Found");
                }
            }

        }

        [HttpPost]
        public JsonResult Add(BoardAdditionViewModel model)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var board = new Board() { Name = model.Name, User = ctx.Users.First(x => x.Username == HttpContext.User.Identity.Name) };
                ctx.Boards.Add(board);
                ctx.SaveChanges();

                foreach (var str in model.Stages)
                { 
                    var stage = new Stage();
                    stage.Name = str;
                    stage.Board = ctx.Boards.First(x => x.Name == model.Name);
                    ctx.Stages.Add(stage);
                }

                foreach (var str in model.Pipelines)
                { 
                    var pipeline = new Pipeline();
                    pipeline.Name = str;
                    pipeline.Board = ctx.Boards.First(x => x.Name == model.Name);
                    ctx.Pipelines.Add(pipeline);
                }

                ctx.SaveChanges();

                return Json(new { status = "success", message = board.Name });
            }
        }

        [HttpPost]
        public JsonResult Validate(string name)
        {
            using (var ctx = new PipelinesDBContext())
            {

                if (ctx.Boards.Count() > 0 && ctx.Boards.Select(x => x.Name.Trim()).Contains(name.Trim()))
                {
                    return Json(new { status = "true" }); //taken
                }

                return Json(new { status = "false" }); //not taken
            }

        }
    }
}
