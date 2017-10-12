using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Starcounter;

namespace StarCounter.App.Service.UserAdmin {
    public class SystemUserGroupsHandlers {
        public static void Register() {

            // Create System User group
            Handle.GET("/UserAdmin/admin/createusergroup", (Request request) => {
                MasterPage master = LauncherHooks.GetMaster();
                Json page;

                if (
                    !Helper.TryNavigateTo("/UserAdmin/admin/createusergroup", request,
                        "/useradmin/viewmodels/RedirectPage.html", out page))
                {
                    master.CurrentPage = page;
                }
                else
                {
                    master.CurrentPage = new CreateUserGroupPage() { Html = "/UserAdmin/viewmodels/partials/administrator/CreateUserGroupPage.html", Uri = request.Uri };
                }

                return master;
            });

            // Get System user groups
            Handle.GET("/UserAdmin/admin/usergroups", (Request request) => {
                MasterPage master = LauncherHooks.GetMaster();
                Json page;

                if (
                    !Helper.TryNavigateTo("/UserAdmin/admin/usergroups", request,
                        "/useradmin/viewmodels/RedirectPage.html", out page))
                {
                    master.CurrentPage = page;
                }
                else
                {
                    master.CurrentPage = new ListUserGroupsPage() { Html = "/UserAdmin/viewmodels/partials/administrator/ListUserGroupsPage.html", Uri = request.Uri };
                }

                return master;
            });

            // Get System user group
            Handle.GET("/UserAdmin/admin/usergroups/{?}", (string usergroupid, Request request) => {
                MasterPage master = LauncherHooks.GetMaster();
                Json page;

                if (!Helper.TryNavigateTo("/UserAdmin/admin/usergroups/{?}", request, "/useradmin/viewmodels/RedirectPage.html", out page)) {
                    master.CurrentPage = page;
                    return master;
                }

                Simplified.Ring3.SystemUserGroup usergroup = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID=?", usergroupid).First;

                if (usergroup == null) {
                    // TODO: Return a "User Group not found" page
                    return master;
                    //return (ushort)System.Net.HttpStatusCode.NotFound;
                }

                master.CurrentPage = Db.Scope(() => {
                    var editUserGroupPage = new EditUserGroupPage
                    {
                        Html = "/UserAdmin/viewmodels/partials/administrator/EditUserGroupPage.html",
                        Uri = request.Uri,
                        Data = usergroup
                    };
                    return editUserGroupPage;
                });
                return master;
            });
        }
    }
}
