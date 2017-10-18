using System;
using System.Linq;
using Simplified.Ring2;
using Simplified.Ring3;
using Starcounter;
using Smorgasbord.PropertyMetadata;

namespace StarCounter.App.Service.UserAdmin
{
    partial class CreateUserPage : PropertyMetadataPage, IBound<SystemUser>
    {
        void Handle(Input.Username action)
        {
            this.AssurePropertyMetadataName(action.Template.TemplateName, action.Value);
        }

        void Handle(Input.FirstName action)
        {
            this.AssurePropertyMetadataName(action.Template.TemplateName, action.Value);
        }

        void Handle(Input.LastName action)
        {
            this.AssurePropertyMetadataName(action.Template.TemplateName, action.Value);
        }

        /// <summary>
        /// Save event
        /// </summary>
        /// <param name="action"></param>
        void Handle(Input.Save action)
        {
            this.AssurePropertiesMetadata();

            if (this.IsInvalid)
            {
                return;
            }

            try
            {
                SystemUserAdmin.AssignPerson(this.Data, this.FirstName, this.LastName);
                Transaction.Commit();
                this.RedirectUrl = "/UserAdmin/admin/users";
            }
            catch (Exception e)
            {
                this.Message = e.Message;
            }
        }

        void Handle(Input.Close action)
        {
            this.RedirectUrl = "/UserAdmin/admin/users";
        }

        void Handle(Input.PersonName action)
        {
            this.FoundPersons.Clear();
            this.Data.WhatIs = null;

            if (!string.IsNullOrEmpty(action.Value))
            {
                this.FoundPersons.Data = Db.SQL<Person>("SELECT p FROM Simplified.Ring2.Person p WHERE p.FullName LIKE ? ORDER BY p.Name", "%" + action.Value + "%")
                    .Where(x => { return Db.SQL<SystemUser>("SELECT u FROM Simplified.Ring3.SystemUser u WHERE u.WhatIs = ?", x).First == null; });
            }
        }

        void Handle(Input.ClearFoundPersons action)
        {
            this.FoundPersons.Clear();
            if (this.Data.WhatIs == null)
            {
                this.PersonName = string.Empty;
            }
        }

        #region Validate Properties (Create Property metadata)

        /// <summary>
        /// Assure metadata for all fields
        /// </summary>
        protected void AssurePropertiesMetadata()
        {
            AssurePropertyMetadataName("Username$", this.Username);
            AssurePropertyMetadataName("FirstName$", this.FirstName);
            AssurePropertyMetadataName("LastName$", this.LastName);
        }

        protected void AssurePropertyMetadataName(string propertyName, string value)
        {
            string message;

            this.ValidateName(value, out message);

            if (message != null)
            {
                PropertyMetadataItem item = new PropertyMetadataItem();
                item.Message = message;
                item.ErrorLevel = 1;
                item.PropertyName = propertyName;
                this.AddPropertyFeedback(item);
            }
            else
            {
                this.RemovePropertyFeedback(propertyName);
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
        private bool ValidateName(string value, out string message)
        {
            message = null;

            if (value == null || string.IsNullOrEmpty(value.Trim()))
            {
                message = "Field can not be empty";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate email
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ValidateEmail(string value, out string message)
        {
            message = null;

            if (!Utils.IsValidEmail(value))
            {
                message = "Invalid email address";
                return false;
            }

            return true;
        }
        #endregion
    }

    [CreateUserPage_json.FoundPersons]
    public partial class CreateUserPageFoundPersons : Json, IBound<Simplified.Ring2.Person>
    {
        void Handle(Input.Choose action)
        {
            this.ParentPage.PersonName = this.Name;
            this.ParentPage.FirstName = this.Data.FirstName;
            this.ParentPage.LastName = this.Data.LastName;
            this.ParentPage.Data.WhatIs = this.Data;
            this.ParentPage.FoundPersons.Clear();
        }

        CreateUserPage ParentPage
        {
            get
            {
                return this.Parent.Parent as CreateUserPage;
            }
        }
    }
}
