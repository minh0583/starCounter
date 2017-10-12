using Starcounter;

namespace StarCounter.App.Service.UserAdmin {
    [SearchResultPage_json]
    partial class SearchResultPage : Json {

        [SearchResultPage_json.Users]
        partial class UserItem : Json {
            public string UserUri {
                get {
                    return "/UserAdmin/admin/users/" + Data.GetObjectID();
                }
            }
        }

        [SearchResultPage_json.Groups]
        partial class GroupItem : Json {
            public string GroupUri {
                get {
                    return "/UserAdmin/admin/usergroups/" + Data.GetObjectID();
                }
            }
        }
    }
}
