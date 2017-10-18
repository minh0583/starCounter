using System.Collections;
using System.Collections.Generic;
using Simplified.Ring3;
using Starcounter;
using Smorgasbord.PropertyMetadata;

namespace StarCounter.App.Service.UserAdmin
{
    [SomebodyPage_json]
    partial class SomebodyPage : PropertyMetadataPage, IBound<SystemUser>
    {
        public string SelectedSystemUserGroupID_;
        public IEnumerable SystemUserGroups_
        {
            get
            {
                List<SystemUserGroup> notmemberofgroups = new List<SystemUserGroup>();

                var groups = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o");
                foreach (var group in groups)
                {

                    var memberOfGroup = Db.SQL<SystemUserGroupMember>("SELECT o FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.SystemUserGroup=? AND o.SystemUser=?", group, this.Data).First;
                    if (memberOfGroup == null)
                    {
                        notmemberofgroups.Add(group);
                    }
                }
                return notmemberofgroups;
            }
        }

        void Handle(Input.AddUserToGroup action)
        {
            this.Message = null;
            if (string.IsNullOrEmpty(this.SelectedSystemUserGroupID_))
            {
                action.Cancel();
                this.Message = "None of the System Groups is selected!";
                return;
            }

            SystemUserGroup group = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID=?", this.SelectedSystemUserGroupID_).First;

            SystemUserGroupMember systemUserGroupMember = new SystemUserGroupMember();

            systemUserGroupMember.WhatIs = this.Data;
            systemUserGroupMember.ToWhat = group;

            this.SelectedSystemUserGroupID_ = null;
        }

        /// <summary>
        /// EMailAddress
        /// </summary>
        //private Simplified.Ring3.EmailAddress EMailAddress {
        //    get {
        //        Simplified.Ring3.SystemUser user = this.Data as Simplified.Ring3.SystemUser;
        //        if (user == null) return null;
        //        return Db.SQL<Simplified.Ring3.EmailAddress>("SELECT o FROM Simplified.Ring3.EmailAddress o WHERE o.ToWhat=?", user).First;
        //    }
        //}

        #region View-model Handlers

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Delete action)
        {
            this.Message = null;
            var transaction = this.Transaction;
            SystemUser user = this.Data;
            SystemUser systemUser = Helper.GetCurrentSystemUser();
            if (systemUser.Equals(user))
            {
                // TODO: Show error message "Can not delete yourself"
                return;
            }

            this.Data = null;
            // TODO: Warn user with Yes/No dialog
            transaction.Rollback();

            transaction.Scope(() =>
            {
                SystemUserAdmin.DeleteSystemUser(user);
            });

            transaction.Commit();

            this.RedirectUrl = "/UserAdmin/admin/users";
        }

        /// <summary>
        /// Save changes
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Save action)
        {
            this.Message = null;
            this.AssurePropertyFeedbacks();

            if (this.IsInvalid)
            {
                return;
            }

            this.SaveChanges();
        }

        /// <summary>
        /// Close page
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Close action)
        {
            if (this.Transaction.IsDirty)
            {
                this.Transaction.Rollback();
            }

            this.RedirectUrl = "/UserAdmin/admin/users";
        }

        #endregion

        protected virtual void AssurePropertyFeedbacks()
        {
        }

        protected virtual void SaveChanges()
        {
            if (this.Transaction.IsDirty)
            {
                this.Transaction.Commit();
            }

            this.RedirectUrl = "/UserAdmin/admin/users";
        }
    }

    // TODO: AWA
    //[SomebodyPage_json.Groups]
    //partial class SombodyGroupItem : Json {

    //    void Handle(Input.Remove action) {

    //        Simplified.Ring3.SystemUserGroup group = this.Data as Simplified.Ring3.SystemUserGroup;
    //        Simplified.Ring3.SystemUser user = this.Parent.Parent.Data as Simplified.Ring3.SystemUser;
    //        var removeGroup = Db.SQL<Simplified.Ring3.SystemUserGroupMember>("SELECT o FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.WhatIs=? AND o.ToWhat=?", user, group).First;
    //        if (removeGroup != null) {
    //            removeGroup.Delete();
    //        }
    //    }
    //}
}
