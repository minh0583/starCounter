using Starcounter;
using System.Collections;

namespace StarCounter.App.Service.UserAdmin {

    [ListUserGroupsPage_json]
    partial class ListUserGroupsPage : Json {

        public IEnumerable Groups {

            get {
                return Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o");
            }
        }
    }

    [ListUserGroupsPage_json.Items]
    partial class SystemUserGroupsItem : Json, IBound<Simplified.Ring3.SystemUserGroup> {

        public Simplified.Ring3.SystemUserGroup BasedOn_ {
            get {
                if (this.Data == null || this.Data.Parent == null) return null;

                return Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o=?",this.Data.Parent).First;
            }
        }

        void Handle(Input.Delete action) {

            // TODO: Warning user with Yes/No dialog
            Db.Transact(() => {
                SystemUserAdmin.DeleteSystemUserGroup(this.Data as Simplified.Ring3.SystemUserGroup);
            });
        }

        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/usergroups/{0}", this.Key);
        }
    }

    [ListUserGroupsPage_json.Items.BasedOn]
    partial class SystemUserGroupsItemBasedOn : Json {
        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/usergroups/{0}", this.Key);
        }
    }
}
