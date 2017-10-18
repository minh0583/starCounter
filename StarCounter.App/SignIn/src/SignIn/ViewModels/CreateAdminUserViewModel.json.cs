using StarCounter.App.Service.SignIn.Helpers;
using Simplified.Ring3;
using Starcounter;
using StarCounter.App.Service.SignIn.Models;
using System;

namespace StarCounter.App.Service.SignIn.ViewModels
{
    partial class CreateAdminUserViewModel : Json, IBound<SystemAdminUser>
    {
        private void Handle(Input.Password action)
        {
            this.Password = action.Value;
            this.Message = this.Data.GetPasswordValidationMessage();
            this.IsAlert = !string.IsNullOrEmpty(this.Message);
 
        }
        private void Handle(Input.PasswordRepeat action)
        {
            this.PasswordRepeat = action.Value;
            this.Message = this.Data.GetPasswordRepeatValidationMessage();
            this.IsAlert = !string.IsNullOrEmpty(this.Message);
        }
        private void Handle(Input.OkTrigger action)
        {
            CreateAdminUser();
        }
        private void Handle(Input.CancelTrigger action)
        {
            GoBack();
        }
        private void Handle(Input.BackTrigger action)
        {
            GoBack();
        }
     
        private void GoBack()
        {
            this.RedirectUrl = "/signin/signinuser";
        }

        private void CreateAdminUser()
        {
            string message = string.Empty;
            bool isAlert = false;
            this.Data.CreateAdminUser(out message, out isAlert);
            this.Message = message;
            this.IsAlert = isAlert;
        }
    }
}
