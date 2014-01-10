using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pipelines.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
    name: "Board",
    url: "Board/Add",
    defaults: new { controller = "Board", action = "Add", name = UrlParameter.Optional }
);
            routes.MapRoute(
    name: "BoardValidation",
    url: "Board/Validate",
    defaults: new { controller = "Board", action = "Validate", name = UrlParameter.Optional }
);
            routes.MapRoute(
    name: "Card",
    url: "Card/{action}",
    defaults: new { controller = "Card", action = "Add" }
);
            routes.MapRoute(
                name: "Pipline",
                url: "Pipeline/{action}",
                defaults: new { controller = "Pipeline", action = "Add" }
            );
            routes.MapRoute(
                name: "Stage",
                url: "Stage/{action}",
                defaults: new { controller = "Stage", action = "Add" }
            );



            routes.MapRoute(
                name: "Default",
                url: "{controller}/{name}",
                defaults: new { controller = "Home", action = "Index", name = UrlParameter.Optional }
            );



            
            routes.MapRoute(
                name: "Subscriptions",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Card", action = "Subscribe", id = UrlParameter.Optional }
            );



        }
    }
}