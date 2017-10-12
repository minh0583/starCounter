using System.Web;
using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;

namespace StarCounter.App.Service.UserAdmin
{
    public class Helper {

        static public bool CanGetUri(SystemUser user, string uri, Request request) {

            // Check if there is any permission set for a url
            UriPermission per = Db.SQL<UriPermission>("SELECT o FROM  Simplified.Ring5.UriPermission o WHERE o.Uri=?", uri).First;
            if (per == null) {

                // TODO: Check if user is part of Admin group, then allow acces?

                // No permission configuration for this url = DENY ACCESS
                return false;
            }

            UriPermission permission = Helper.GetPermission(user, uri);
            if (permission != null) {
                return permission.CanGet;
            }

            return false;
        }

        static private UriPermission GetPermission(SystemUser user, string uri) {

            if (user == null || string.IsNullOrEmpty(uri)) {
                return null;
            }

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Simplified.Ring5.SystemUserUriPermission o WHERE o.Permission.Uri=? AND o.SystemUser=?", uri, user).First;
            if (permission != null) {
                return permission;
            }

            // Check user group
            var groups = Db.SQL<Simplified.Ring3.SystemUserGroupMember>("SELECT o FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.SystemUser=?", user);
            foreach (var group in groups) {

                permission = GetPermissionFromGroup(group.SystemUserGroup, uri);
                if (permission != null) {
                    return permission;
                }
            }
            return null;
        }

        static private UriPermission GetPermissionFromGroup(SystemUserGroup group, string url) {

            if (group == null) return null;

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Simplified.Ring5.SystemUserGroupUriPermission o WHERE o.Permission.Uri=? AND o.SystemUserGroup=?", url, group).First;
            if (permission != null) {
                return permission;
            }

            permission = GetPermissionFromGroup(group.Parent, url);
            if (permission != null) {
                return permission;
            }

            return null;
        }

        //static public bool IsMemberOfAdminGroup(Simplified.Ring3.SystemUser user) {

        //    if (user == null) return false;
        //    Simplified.Ring3.SystemUser Group adminGroup = Db.SQL<Simplified.Ring3.SystemUser	Group>("SELECT o FROM Simplified.Ring3.SystemUser Group o WHERE o.Name=?", Program.AdminGroupName).First;

        //    return IsMemberOfGroup(user, adminGroup);
        //}

        static public bool IsMemberOfGroup(SystemUser user, SystemUserGroup basedOnGroup) {

            if (user == null) return false;
            if (basedOnGroup == null) return false;

            var groups = Db.SQL<SystemUserGroup>("SELECT o.SystemUserGroup FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.SystemUser=?", user);
            foreach (var groupItem in groups) {

                bool flag = IsBasedOnGroup(groupItem, basedOnGroup);
                if (flag) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// TODO: Avoid circular references!!
        /// </summary>
        /// <param name="group"></param>
        /// <param name="basedOnGroup"></param>
        /// <returns></returns>
        static private bool IsBasedOnGroup(SystemUserGroup group, SystemUserGroup basedOnGroup) {
            if (group == null) return false;

            // NOTE: To compare to objects queried from database we need to use .Equals(),  "==" wont work!!.
            if (group.Equals(basedOnGroup)) {
                return true;
            }

            if (IsBasedOnGroup(group.Parent, basedOnGroup)) {
                return true;
            }

            return false;
        }


        static public void AssureUriPermission(string uri, SystemUserGroup group) {

            UriPermission permission = Db.SQL<UriPermission>("SELECT o.Permission FROM Simplified.Ring5.SystemUserGroupUriPermission o WHERE o.Permission.Uri=? AND o.SystemUserGroup=?", uri, group).First;

            if (permission == null) {

                Db.Transact(() => {
                    UriPermission p1 = new UriPermission() { Uri = uri, CanGet = true };
                    new SystemUserGroupUriPermission() { ToWhat = p1, WhatIs = group };
                });
            }
        }


        /// <summary>
        /// Assure that there is at least one system user beloning to the admin group 
        /// </summary>
        /*static public void AssureOneAdminSystemUser(string adminGroupName, string description) {

            SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name=?", adminGroupName).First;

            // Assure that there is at least one system user with admin rights
            var result = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o");
            foreach (var user in result) {

                if (Helper.IsMemberOfGroup(user, adminGroup)) {
                    return;
                }
            }

            // There is no system user beloning to the admin group

            Db.Transact(() => {

                // Assure that there is a Admin group

                if (adminGroup == null) {
                    adminGroup = new SystemUserGroup();
                    adminGroup.Name = adminGroupName;
                    adminGroup.Description = description;
                }

                // Check if there is an "admin" system user
                Simplified.Ring3.SystemUser systemUser = Db.SQL<Simplified.Ring3.SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username=?", "admin").First;
                if (systemUser == null) {

                    string username = "admin";
                    string password = "admin";

                    // Create Person and system user;
                    Person person = new Person() { FirstName = username, LastName = username };
                    systemUser = new Simplified.Ring3.SystemUser();
                    systemUser.WhatIs = person;
                    systemUser.Username = username;

                    // Set password
                    string hashedPassword;
                    Helper.GeneratePasswordHash(systemUser.Username.ToLower(), password, out hashedPassword);
                    systemUser.Password = hashedPassword;


                    // Add ability to also sign in with email
                    EmailAddress emailRel = new EmailAddress();

                    EmailAddressRelation eRel = new EmailAddressRelation();
                    eRel.ToWhat = systemUser;
                    eRel.WhatIs = emailRel;

                    //emailRel.SetToWhat(systemUser);
                    emailRel.EMail = "change@this.email".ToLowerInvariant();


                    //systemUser = SystemUserAdmin.AddPerson("admin", "admin", "admin", "change@this.email", "admin");
                }

                // Add the admin group to the system admin user
                SystemUserGroupMember systemUserGroupMember = new Simplified.Ring3.SystemUserGroupMember();
                systemUserGroupMember.WhatIs = systemUser;
                systemUserGroupMember.ToWhat = adminGroup;

                //systemUserGroupMember.SetSystemUser(systemUser);
                //systemUserGroupMember.SetToWhat(adminGroup);

                //SystemUserAdmin.AddSystemUserToSystemUserGroup(systemUser, adminGroup);
            });

        }*/

        static public bool TryNavigateTo(string url, Request request, string html, out Json returnPage) {

            returnPage = null;

            SystemUser systemUser = Helper.GetCurrentSystemUser();
            if (systemUser == null) {
                // Ask user to sign in.
                returnPage = Helper.GetSignInPage(request.Uri, html);
                return false;
            }

            // Check user permission
            if (!Helper.CanGetUri(systemUser, url, request)) {
                // User has no permission, redirect to app's root page
                returnPage = Helper.GetRedirectPage("/useradmin/accessdenied", html);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static public Json GetSignInPage(string referer, string html) {
            return GetRedirectPage("/useradmin/unauthenticated?return_uri=" + HttpUtility.UrlEncode(referer), html);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static public Json GetRedirectPage(string redirectUrl, string html) {
            return new RedirectPage() { Html = html, RedirectUrl = redirectUrl };
        }


        static public SystemUser GetCurrentSystemUser() {

            if (Session.Current == null) return null;

            SystemUserSession userSession = Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString=?", Session.Current.SessionId).First;
            if (userSession == null) {
                return null;
            }

            if (userSession.Token == null) {
                return null;
            }

            return userSession.Token.User;
        }
    }
}
