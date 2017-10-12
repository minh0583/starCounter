using System;
using System.Net;
using System.Net.Mail;
using Simplified.Ring6;

namespace StarCounter.App.Service.UserAdmin
{
    public class Utils {


        static public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
    }
}
