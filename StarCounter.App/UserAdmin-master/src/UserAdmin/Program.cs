using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;
using Simplified.Ring1;
using Simplified.Ring2;
using Simplified.Ring3;
using Simplified.Ring5;

namespace StarCounter.App.Service.UserAdmin
{
    public class Program
    {
        internal static string AdminGroupName = "Admin (System Users)";
        internal static string AdminGroupDescription = "System User Administrator Group";

        static void Main()
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            Program.SetupPermissions();

            SystemUsersHandlers.Register();
            SystemUserGroupsHandlers.Register();
            CommitHooks.Register();
            LauncherHooks.Register();


            // Mapping issue
            // https://github.com/Starcounter/Starcounter/issues/2902
            Blender.MapUri<Person>("/UserAdmin/persons/{?}",
                paramsFrom =>
                {
                    var objectId = paramsFrom[0];
                    var user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID=?", objectId).First;
                    if (user != null)
                    {
                        paramsFrom[0] = user.WhatIs.Key;
                    }

                    return null;
                },
                paramsTo =>
                {
                    var objectId = paramsTo[0];
                    var user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.WhatIs.ObjectID=?", objectId).First;

                    if (user != null)
                    {
                        paramsTo[0] = user.Key;
                        return paramsTo;
                    }
                    return null;
                }
            );
        }

        /// <summary>
        /// Set up Uri permissions
        /// TODO: This is hardcoded, we need a gui!!
        /// TODO: Automate this
        /// </summary>
        static private void SetupPermissions()
        {

            SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?", Program.AdminGroupName).First;

            if (adminGroup == null)
            {
                Db.Transact(() =>
                {
                    adminGroup = new SystemUserGroup();
                    adminGroup.Name = AdminGroupName;
                    adminGroup.Description = AdminGroupDescription;
                });
            }

            Helper.AssureUriPermission("/UserAdmin/admin/users", adminGroup);
            Helper.AssureUriPermission("/UserAdmin/admin/users/{?}", adminGroup);
            Helper.AssureUriPermission("/UserAdmin/admin/createuser", adminGroup);

            Helper.AssureUriPermission("/UserAdmin/admin/usergroups", adminGroup);
            Helper.AssureUriPermission("/UserAdmin/admin/usergroups/{?}", adminGroup);
            Helper.AssureUriPermission("/UserAdmin/admin/createusergroup", adminGroup);
        }
    }
}
