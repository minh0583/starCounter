using System;
using Starcounter;
using Simplified.Ring5;

namespace StarCounter.App.Service.UserAdmin {
    public class CommitHooks {
        private static bool oldIsAdmin = false;

        internal static void Register() {
            Hook<SystemUserSession>.CommitInsert += (s, a) => {
                RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitUpdate += (s, a) => {
                RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitDelete += (s, a) => {
                RefreshSignInState();
            };
        }

        private static void RefreshSignInState() {
            bool isAdmin = MasterPage.IsAdmin();

            MasterPage master = Session.Current.Data as MasterPage; ;
            if (master == null) return;

            AdminMenu page = master.Menu as AdminMenu;
            if (page == null) {
                return;
            }

            page.IsAdministrator = isAdmin;

            if (!isAdmin && oldIsAdmin) {
                page.RedirectUrl = "/launcher";
            }

            oldIsAdmin = isAdmin;
        }
    }
}
