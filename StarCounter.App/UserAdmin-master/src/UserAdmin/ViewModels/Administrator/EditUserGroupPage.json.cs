using Smorgasbord.PropertyMetadata;
using Starcounter;
using System;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Web;

// FORGOT PASSWORD:
// http://www.asp.net/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity

namespace StarCounter.App.Service.UserAdmin {

    [EditUserGroupPage_json]
    partial class EditUserGroupPage : PropertyMetadataPage, IBound<Simplified.Ring3.SystemUserGroup> {

        #region Properties

        void Handle(Input.Name action) {

            this.AssurePropertyMetadata_Name(action.Template.TemplateName, action.Value);
        }

        #region Set Based On System User group

        /// <summary>
        /// Selected System User group (dropdown)
        /// </summary>
        public string SelectedBasedOnGroupID_ {

            get {

                if (this.Data.Parent == null) return null;

                Simplified.Ring3.SystemUserGroup group = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID=?", this.Data.Parent.GetObjectID()).First;
                if (group == null) return null;

                return group.GetObjectID();
            }
            set {

                if (string.IsNullOrEmpty(value)) {
                    this.Data.Parent = null;
                }

                Simplified.Ring3.SystemUserGroup group = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID=?", value).First;
                this.Data.Parent = group;
            }
        }

        /// <summary>
        /// System User Groups (exept the current one)
        /// </summary>
        public IEnumerable Groups {
            get {

                // TODO: do not show groups that this group is inhereted
                return Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID<>? ORDER BY o.Name", ((Simplified.Ring3.SystemUserGroup)this.Data).GetObjectID());
            }
        }

        #endregion

        #endregion

        #region View-model Handlers

        /// <summary>
        /// BasedOn changed
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.SelectedBasedOnGroupID action) {

            Simplified.Ring3.SystemUserGroup group = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.ObjectID=?", action.Value).First;

            // Check for circular references?
            while (group != null) {

                if (group.Equals(this.Data)) {
                    this.Message = "Circular referenced system groups are not allowd";
                    action.Cancel();
                    action.Cancelled = true;
                    this.SelectedBasedOnGroupID = action.OldValue;
                    return;
                }

                group = group.Parent;
            }

            this.Message = string.Empty;
        }

        /// <summary>
        /// AddUser operation invoked
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.AddUser action) {


            this.AssurePropertyMetadata_AddUserName("AddUserName$", this.AddUserName);
            if (this.AddUser_Feedback != null) {
                return;
            }

            Simplified.Ring3.SystemUser systemUser = Db.SQL<Simplified.Ring3.SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username=?", this.AddUserName).First;
            if (systemUser == null) {
                PropertyMetadataItem item = new PropertyMetadataItem();
                item.Message = "User not found";
                item.ErrorLevel = 1;
                item.PropertyName = "AddUser$";
                this.AddUser_Feedback = item;
                return;
            }

            SystemUserAdmin.AddSystemUserToSystemUserGroup(systemUser, this.Data);
            this.AddUserName = string.Empty;
        }

        /// <summary>
        /// Delete System User Group
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Delete action) {

            // TODO: Warn user with Yes/No dialog
            var transaction = this.Transaction;
            transaction.Scope(() => {
                SystemUserAdmin.DeleteSystemUserGroup(this.Data as Simplified.Ring3.SystemUserGroup);
            });

            if (transaction.IsDirty) {
                transaction.Commit();
            }

            this.RedirectUrl = "/UserAdmin/admin/usergroups";
        }

        /// <summary>
        /// Save changes
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Save action) {

            this.AssurePropertiesMetadata();

            if (this.IsInvalid) {
                return;
            }

            if (this.Transaction.IsDirty) {
                this.Transaction.Commit();
            }

            this.RedirectUrl = "/UserAdmin/admin/usergroups";
        }

        /// <summary>
        /// Close page
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Close action) {

            if (this.Transaction.IsDirty) {
                this.Transaction.Rollback();
            }
            this.RedirectUrl = "/UserAdmin/admin/usergroups";
        }

        #endregion


        #region Validate Properties (Create Property metadata)

        /// <summary>
        /// Assure metadata for all fields
        /// </summary>
        virtual protected void AssurePropertiesMetadata() {

            AssurePropertyMetadata_Name("Name$", this.Name);
        }

        protected void AssurePropertyMetadata_Name(string propertyName, string value) {

            string message;
            this.Validate_Name(value, out message);
            if (message != null) {
                PropertyMetadataItem item = new PropertyMetadataItem();
                item.Message = message;
                item.ErrorLevel = 1;
                item.PropertyName = propertyName;
                this.AddPropertyFeedback(item);
            }
            else {
                this.RemovePropertyFeedback(propertyName);
            }
        }

        protected void AssurePropertyMetadata_AddUserName(string propertyName, string value) {

            string message;
            this.Validate_AddUserName(value, out message);
            if (message != null) {
                PropertyMetadataItem item = new PropertyMetadataItem();
                item.Message = message;
                item.ErrorLevel = 1;
                item.PropertyName = propertyName;
                this.AddUser_Feedback = item;
            }
            else {
                this.AddUser_Feedback = null;
            }
        }

        #endregion

        #region Validate Properties

        /// <summary>
        /// Validate name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool Validate_Name(string value, out string message) {

            message = null;
            if (value == null || string.IsNullOrEmpty(value.Trim())) {
                message = "Field can not be empty";
                return false;
            }

            var result = Db.SQL<Simplified.Ring3.SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name=? AND o<>?", value, this.Data).First;
            if (result != null) {
                message = "System group with that name already exists";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool Validate_AddUserName(string value, out string message) {

            message = null;
            if (value == null || string.IsNullOrEmpty(value.Trim())) {
                message = "Field can not be empty";
                return false;
            }

            Simplified.Ring3.SystemUser systemUser = Db.SQL<Simplified.Ring3.SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.Username=?", value).First;
            if (systemUser == null) {
                message = "User not found";
                return false;
            }

            // Check if user is alread a member of this sytem user group
            Simplified.Ring3.SystemUserGroup group = this.Data as Simplified.Ring3.SystemUserGroup;

            foreach (var member in group.Members) {
                if (member.Username.Equals(value, StringComparison.InvariantCultureIgnoreCase)) {
                    message = "User is already member of the group";
                    return false;
                }
            }
            return true;
        }

        #endregion
    }

    [EditUserGroupPage_json.Members]
    partial class UserGroupMember : Json {

        /// <summary>
        /// Remove a SystemUser from the group
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Remove action) {
            Simplified.Ring3.SystemUserGroup group = this.Parent.Parent.Data as Simplified.Ring3.SystemUserGroup;
            Simplified.Ring3.SystemUser systemUser = this.Data as Simplified.Ring3.SystemUser;
            SystemUserAdmin.RemoveSystemUserFromSystemUserGroup(systemUser, group);
        }

        protected override void OnData() {
            base.OnData();
            this.Url = string.Format("/useradmin/admin/users/{0}", this.Key);
        }
    }
}
