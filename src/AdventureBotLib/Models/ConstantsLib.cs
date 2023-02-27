using System.Net.Mail;

namespace AdventureBot.Models;

public static class ConstantsLib
{
    public static bool IsValidEmail(string email)
    {
        if(string.IsNullOrEmpty(email))
        {
            return false;
        }
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".")) {
            return false; // suggested by @TK-421
        }
        if(!trimmedEmail.Contains("@"))
            return false;
        var domain = trimmedEmail.Split("@")[1];
        if(!domain.Contains("."))
            return false;
        try {
            var addr = new MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch {
            return false;
        }
    }
}