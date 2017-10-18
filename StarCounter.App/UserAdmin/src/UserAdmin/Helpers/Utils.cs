using System.Net.Mail;

namespace StarCounter.App.Service.UserAdmin
{
    public class Utils {


        static public bool IsValidEmail(string email) {
            try {
                var addr = new MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
    }
}
