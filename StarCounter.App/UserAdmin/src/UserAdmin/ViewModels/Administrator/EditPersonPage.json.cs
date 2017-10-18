using Simplified.Ring2;
using Simplified.Ring3;
using Starcounter;

namespace StarCounter.App.Service.UserAdmin
{
    [EditPersonPage_json]
    partial class EditPersonPage : SomebodyPage, IBound<SystemUser>
    {
        void Handle(Input.FirstName action)
        {
            this.Message = null;
            this.AssurePropertyFeedback_Name("FirstName$", action.Value);
        }

        void Handle(Input.LastName action)
        {
            this.Message = null;
            this.AssurePropertyFeedback_Name("LastName$", action.Value);
        }

        #region Validate Properties (Create Property Feedbacks)

        protected void AssurePropertyFeedback_Name(string propertyName, string value)
        {
            string message;
            this.Validate_Name(value, out message);
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

        private bool Validate_Name(string value, out string message)
        {
            message = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                message = "Field can not be empty";
                return false;
            }
            return true;
        }

        protected override void AssurePropertyFeedbacks()
        {
            Person person = this.Data.WhoIs as Person;
            if (person == null)
                return;

            AssurePropertyFeedback_Name("FirstName$", person.FirstName);
            AssurePropertyFeedback_Name("LastName$", person.LastName);

            base.AssurePropertyFeedbacks();
        }

        #endregion
    }
}
