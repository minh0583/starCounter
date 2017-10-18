using System;
using System.Web;
using Simplified.Ring3;
using Simplified.Ring6;
using Smorgasbord.PropertyMetadata;
using Starcounter;

// FORGOT PASSWORD:
// http://www.asp.net/identity/overview/features-api/account-confirmation-and-password-recovery-with-aspnet-identity

namespace StarCounter.App.Service.SignIn
{
    partial class SystemUserAuthenticationSettings : PropertyMetadataPage, IBound<SystemUser>
    {
        public bool ResetPassword_Enabled_
        {
            get
            {
                var emailAddress = Utils.GetUserEmailAddress(this.Data);
                return emailAddress != null && MailSettingsHelper.GetSettings().Enabled && Utils.IsValidEmail(emailAddress.EMail);
            }
        }

        void Handle(Input.ResetPassword action)
        {
            // Go to "Reset password" form
            this.Message = null;
            this.ResetUserPassword();
        }

        protected void ResetUserPassword()
        {
            string link = null;
            string fullName = string.Empty;
            var mailSettings = MailSettingsHelper.GetSettings();

            if (mailSettings.Enabled == false)
            {
                this.Message = "Mail Server not enabled in the settings.";
                return;
            }

            if (string.IsNullOrEmpty(mailSettings.SiteHost))
            {
                this.Message = "Invalid settings, check site host name / port";
                return;
            }

            var emailAddress = Utils.GetUserEmailAddress(this.Data);
            var email = emailAddress.EMail;
            if (!Utils.IsValidEmail(email))
            {
                this.Message = "Username is not an email address";
                return;
            }

            var transaction = this.Transaction;
            transaction.Scope(() =>
            {
                SystemUser systemUser = this.Data;
                // Generate Password Reset token
                ResetPassword resetPassword = new ResetPassword()
                {
                    User = systemUser,
                    Token = HttpUtility.UrlEncode(Guid.NewGuid().ToString()),
                    Expire = DateTime.UtcNow.AddMinutes(1440)
                };

                // Get FullName
                if (systemUser.WhoIs != null)
                {
                    fullName = systemUser.WhoIs.FullName;
                }
                else
                {
                    fullName = systemUser.Username;
                }

                // Build reset password link
                UriBuilder uri = new UriBuilder();

                uri.Host = mailSettings.SiteHost;
                uri.Port = (int)mailSettings.SitePort;

                uri.Path = "signin/user/resetpassword";
                uri.Query = "token=" + resetPassword.Token;

                link = uri.ToString();
            });

            transaction.Commit();

            try
            {
                this.Message = string.Format("Sending mail sent to {0}...", email);
                Utils.SendResetPasswordMail(fullName, email, link);
                this.Message = "Mail sent.";
            }
            catch (Exception e)
            {
                this.Message = e.Message;
            }
        }

    }
}
