using Simplified.Ring5;
using Starcounter;

namespace StarCounter.App.Service.SignIn
{
    partial class SignInFormPage : Json
    {
        protected override void OnData()
        {
            base.OnData();
            this.SessionUri = Session.Current.SessionUri;
        }

        void Handle(Input.SignInClick action)
        {
            this.Message = null;
            action.Cancel();

            this.Submit++;

            RedirectUrl = "/chatter";
        }

        void Handle(Input.RestoreClick action)
        {
            action.Cancel();

            if (this.MainForm != null)
            {
                this.MainForm.OpenRestorePassword();
            }
        }
        void Handle(Input.CreateAdminClick action)
        {
            RedirectUrl = "/signin/createadminuser";
        }

        protected MainFormPage MainForm
        {
            get { return this.Parent as MainFormPage; }
        }
    }
}