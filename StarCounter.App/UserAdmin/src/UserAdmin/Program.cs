using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;
using StarCounter.App.Service.UserAdmin.Api;

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
            MainHandlers.Register();
            MappingHandlers.Register();
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
