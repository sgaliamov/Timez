[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Timez.App_Start.Combres), "PreStart")]
namespace Timez.App_Start {
	using System.Web.Routing;
	using global::Combres;
	
    public static class Combres {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}