using Simplified.Ring3;
using Starcounter;
using System.Collections;

namespace StarCounter.App.Service.UserAdmin {

    [ListUsersPage_json]
    partial class ListUsersPage : Json {

        public IEnumerable Users {

            get {
                return Db.SQL<Simplified.Ring3.SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o");
            }
        }
    }

    [ListUsersPage_json.Items]
    partial class SystemUsersItem : Json {

        public string Kind {

            get {

                Simplified.Ring3.SystemUser user = this.Data as Simplified.Ring3.SystemUser;
                if (user != null) {
                    if (user.WhoIs != null) {
                        return user.WhoIs.GetType().Name;
                    }
                    else {
                        return user.GetType().Name;
                    }
                }
                return null;
            }
        }

        void Handle(Input.Delete action) {

            SystemUser systemUser = Helper.GetCurrentSystemUser();
            if (systemUser.Equals(this.Data)) {
                // TODO: Show error message "Can not delete yourself"
                return;
            }

            // TODO: Warning user with Yes/No dialog
            Db.Transact(() => {
                SystemUserAdmin.DeleteSystemUser(this.Data as Simplified.Ring3.SystemUser);
            });
        }

        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/users/{0}", this.Key);
        }
    }

    [ListUsersPage_json.Items.Groups]
    partial class SystemUsersItemGroups : Json {
        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/usergroups/{0}", this.Key);
        }
    }
}
