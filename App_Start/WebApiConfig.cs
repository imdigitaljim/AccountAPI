using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace AccountManager
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
            name: "AccountGetPost",
            routeTemplate: "blizzard/{controller}",
            defaults: "",
            constraints: new { httpMthod = new HttpMethodConstraint(HttpMethod.Get, HttpMethod.Post) }
            );

            config.Routes.MapHttpRoute(
            name: "AccountDelete",
            routeTemplate: "blizzard/{controller}/{account_name}",
            defaults: "",
            constraints: new { httpMthod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            config.Routes.MapHttpRoute(
            name: "CharacterDelete",
            routeTemplate: "blizzard/{controller}/{account_name}/characters/{character_name}",
            defaults: "",
            constraints: new { httpMthod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            config.Routes.MapHttpRoute(
            name: "CharacterGetPost",
            routeTemplate: "blizzard/{controller}/{account_name}/characters",
            defaults: "",
            constraints: new { httpMthod = new HttpMethodConstraint(HttpMethod.Get, HttpMethod.Post) }

            );

        }
    }
}
