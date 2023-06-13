using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;

namespace Budget_Api
{
    public class Global : HttpApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configuration.Formatters
                .Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_PreSendRequestHeaders()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Headers.Remove("Server");
            }
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);//Removing Private from cache
            Response.AppendHeader("Cache-Control", "no-cache"); //HTTP 1.1
          //  Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.1
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.Now);
            Response.Cache.SetValidUntilExpires(true);
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "SAMEORIGIN"); //SAMEORIGIN
            //HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");//restrict you website to be used inside IFRAME.
            Response.Cache.AppendCacheExtension("must-revalidate");
            Response.Cache.AppendCacheExtension("pre-check=0");
            Response.Cache.AppendCacheExtension("post-check=0");
            Response.Cache.AppendCacheExtension("max-age=0");
            Response.Cache.AppendCacheExtension("s-maxage=0");
        }

    }
}