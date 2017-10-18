using Starcounter;

namespace StarCounter.App.Service.UserAdmin.ViewModels.Administrator {
    partial class SystemUserGroupItem : Json {

        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/usergroups/{0}", this.Key);
        }

        void Handle(Input.Remove action) {

            Simplified.Ring3.SystemUserGroup group = this.Data as Simplified.Ring3.SystemUserGroup;
            Simplified.Ring3.SystemUser user = this.Parent.Parent.Data as Simplified.Ring3.SystemUser;
            var removeGroup = Db.SQL<Simplified.Ring3.SystemUserGroupMember>("SELECT o FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.WhatIs=? AND o.ToWhat=?", user, group).First;
            if (removeGroup != null) {
                removeGroup.Delete();
            }
        }


    }
}
