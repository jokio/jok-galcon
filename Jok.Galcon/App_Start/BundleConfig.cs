using System.Web;
using System.Web.Optimization;

namespace Jok.StarWars
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            
            bundles.Add(new StyleBundle("~/play/css").Include(
                "~/Content/site.css"
            ));

            bundles.Add(new ScriptBundle("~/play/js").Include(
                "~/Scripts/kinetic-v4.4.3.js",
                "~/Scripts/jquery.signalR-2.0.0.js",
                "~/Scripts/Jok.GameEngine.js",
                "~/Scripts/Game.js"
            ));
        }
    }
}
