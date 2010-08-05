using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Spark.Web.Mvc;
using Spark;
using Spark.Extensions;

namespace ClientSideValidation
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            var sparkSettings = (new SparkSettings())
                .SetAutomaticEncoding(true);


            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new SparkViewFactory { Engine = { ExtensionFactory = new ClientSideValidationSparkExtensionFactory() } });
            


            RegisterRoutes(RouteTable.Routes);

            DataAnnotationsModelValidatorProvider
                .AddImplicitRequiredAttributeForValueTypes = false;
        }
    }
}