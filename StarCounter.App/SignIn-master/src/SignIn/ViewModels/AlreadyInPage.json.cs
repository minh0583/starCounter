using Starcounter;
using Simplified.Ring3;

namespace StarCounter.App.Service.SignIn
{
    partial class AlreadyInPage : Json
    {
        protected override void OnData()
        {
            base.OnData();

            SystemUser user = SystemUser.GetCurrentSystemUser();

            if (user != null)
            {
                this.Username = user.Username;
            }
        }
    }
}