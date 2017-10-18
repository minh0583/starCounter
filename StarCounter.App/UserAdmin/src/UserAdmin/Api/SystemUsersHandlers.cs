using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;

namespace StarCounter.App.Service.UserAdmin
{
    public class SystemUsersHandlers
    {
        public static void Register()
        {
            string redirectPageHtml = "/useradmin/viewmodels/RedirectPage.html";

            Handle.GET("/useradmin/accessdenied", () =>
            {
                return new AccessDeniedPage();
            });

            Handle.GET("/useradmin/unauthenticated?return_uri={?}", (string returnUri) =>
            {
                MasterPage master = MainHandlers.GetMasterPageFromSession();
                master.CurrentPage = Self.GET<UnauthenticatedPage>("/useradmin/partial/unauthenticated?return_uri=" + returnUri);
                return master;
            });

            Handle.GET("/useradmin/partial/unauthenticated?return_uri={?}", (string returnUri) =>
            {
                return new UnauthenticatedPage();
            });

            // Create System user
            Handle.GET("/useradmin/admin/createuser", (Request request) =>
            {
                MasterPage master = MainHandlers.GetMasterPageFromSession();

                Json page;
                if (!Helper.TryNavigateTo("/UserAdmin/admin/createuser", request, redirectPageHtml, out page))
                {
                    master.CurrentPage = page;
                }
                else
                {
                    master.CurrentPage = Db.Scope(() =>
                    {
                        var user = new SystemUser();
                        return new CreateUserPage
                        {
                            Html = "/UserAdmin/viewmodels/partials/administrator/CreateUserPage.html",
                            Uri = request.Uri,
                            Data = user,
                            SystemUserPasswordPage =
                                Self.GET("/useradmin/user/authentication/password/" + user.GetObjectID())
                        };
                    });
                }

                return master;
            });

            // Get System users
            Handle.GET("/useradmin/admin/users", (Request request) =>
            {
                MasterPage master = MainHandlers.GetMasterPageFromSession();
                Json page;
                if (!Helper.TryNavigateTo("/useradmin/admin/users", request, redirectPageHtml, out page))
                {
                    master.CurrentPage = page;
                }
                else
                {
                    master.CurrentPage = new ListUsersPage() { Html = "/UserAdmin/viewmodels/partials/administrator/ListUsersPage.html", Uri = request.Uri };
                }
                return master;
            });

            Handle.GET("/UserAdmin/persons/{?}", (string userid) =>
            {
                return Self.GET("/UserAdmin/admin/users/" + userid);
            });

            Handle.GET("/UserAdmin/admin/users/{?}", (string userid, Request request) =>
            {

                Json page;
                MasterPage master = MainHandlers.GetMasterPageFromSession();

                if (!Helper.TryNavigateTo("/UserAdmin/admin/users/{?}", request, redirectPageHtml, out page))
                {
                    master.CurrentPage = page;
                    return master;
                }

                // Get system user
                SystemUser user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID = ?", userid).First;

                if (user == null)
                {
                    // TODO: Return a "User not found" page
                    return master;
                }

                SystemUser systemUser = Helper.GetCurrentSystemUser();
                SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?", Program.AdminGroupName).First;

                // Check if current user has permission to get this user instance
                if (Helper.IsMemberOfGroup(systemUser, adminGroup))
                {
                    if (user.WhoIs is Person)
                    {
                        master.CurrentPage = Db.Scope(() => new EditPersonPage
                        {
                            Html = "/UserAdmin/viewmodels/partials/administrator/EditPersonPage.html",
                            Uri = request.Uri,
                            Data = user,
                            SystemUserAuthenticationSettingsPage =
                                Self.GET("/useradmin/user/authentication/settings/" + user.GetObjectID())
                        });

                        return master;
                    }
                }
                else if (user == systemUser)
                {
                    // User can edit it's self
                }
                else
                {
                    // No rights
                    // User trying to view another's users data

                    // User has no permission, redirect to app's root page
                    master.CurrentPage = new RedirectPage()
                    {
                        Html = redirectPageHtml,
                        RedirectUrl = "/useradmin"
                    };
                    return master;
                }

                return master;
            });

            // User authentication settings
            Handle.GET("/useradmin/user/authentication/settings/{?}", (string userId) => new Json(),
                new HandlerOptions { SelfOnly = true });

            // User password settings
            Handle.GET("/useradmin/user/authentication/password/{?}", (string userId) => new Json(),
                new HandlerOptions { SelfOnly = true });
        }
    }
}
