using Simplified.Ring3;
using Starcounter;

namespace StarCounter.App.Service.UserAdmin {
    partial class MasterPage : Json {
        public Json Menu { get; set; }

        static public bool IsAdmin() {

            SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?", Program.AdminGroupName).First;
            SystemUser user = Helper.GetCurrentSystemUser();

            return Helper.IsMemberOfGroup(user, adminGroup);
        }
    }
}
