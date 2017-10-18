using Starcounter;

namespace StarCounter.App.Service.UserAdmin
{
    public class MainHandlers
    {

        public static MasterPage GetMasterPageFromSession()
        {
            if (Session.Current == null)
            {
                Session.Current = new Session(SessionOptions.PatchVersioning);
            }

            MasterPage master = Session.Current.Data as MasterPage;

            if (master == null)
            {
                master = new MasterPage();
                Session.Current.Data = master;
            }
            master.Html = "/useradmin/viewmodels/launcher/MasterPage.html";
            return master;
        }

        public static void Register()
        {
            Handle.GET("/useradmin/app-name", () =>
            {
                return new AppName();
            });

            Handle.GET("/useradmin", () =>
            {
                return Self.GET("/useradmin/admin/users");
            });

            // Menu
            Handle.GET("/useradmin/menu", () =>
            {

                MasterPage master = GetMasterPageFromSession();

                master.Menu = new AdminMenu() { Html = "/UserAdmin/viewmodels/launcher/AppMenuPage.html", IsAdministrator = MasterPage.IsAdmin() };
                return master.Menu;
            });
            
            Handle.GET("/useradmin/search/{?}", (string query) =>
            {
                var result = new SearchResultPage();
                result.Html = "/UserAdmin/viewmodels/launcher/AppSearchPage.html";

                // If not authorized we don't return any results.
                if (!string.IsNullOrEmpty(query) && MasterPage.IsAdmin())
                {
                    result.Users = Db.SQL<Simplified.Ring3.SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username LIKE ? FETCH ?", "%" + query + "%", 5);
                    result.Groups = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name LIKE ? FETCH ?", "%" + query + "%", 5);
                }

                return result;
            });
        }
    }
}
