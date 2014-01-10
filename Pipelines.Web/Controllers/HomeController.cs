using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pipelines.Web.Models;
using Pipelines.Data.Context;
using Pipelines.Data.Models;
using System.DirectoryServices;

namespace Pipelines.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //TODO: Add user support, we use to use AD here 
            ViewData["email"] = "taygibb@gmail.com";

            using (var ctx = new PipelinesDBContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Username == HttpContext.User.Identity.Name) == null)
                {
                    ctx.Users.Add(new User() { Username = HttpContext.User.Identity.Name, Email = ViewData["email"].ToString() });
                    ctx.SaveChanges();
                }

                return View(ctx.Boards.ToList());   
            }
        }
    }
}
