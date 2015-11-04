using System.Web.Routing;
using AttributeRouting.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AccountManager.AttributeRoutingConfig), "Start")]

namespace AccountManager 
{
    public static class AttributeRoutingConfig
	{
		public static void RegisterRoutes(RouteCollection routes) 
		{        
			routes.MapAttributeRoutes();
		}

        public static void Start() 
		{
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
