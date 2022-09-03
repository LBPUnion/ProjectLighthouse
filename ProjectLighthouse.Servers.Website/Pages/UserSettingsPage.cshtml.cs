#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserSettingsPage : BaseLayout
{

    public User? ProfileUser;
    public UserSettingsPage(Database database) : base(database)
    {}

    private static byte[] ParseBase64(string base64)
    {
        Span<byte> buffer = new(new byte[base64.Length]);
        Convert.TryFromBase64String(base64, buffer, out int _);
        return buffer.ToArray();
    }

    public async Task<IActionResult> OnPost([FromRoute] int userId, [FromForm] string avatar, [FromForm] string username, [FromForm] string email, [FromForm] string biography, [FromForm] string timezone)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);

        if (!this.User.IsModerator || this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        if(string.IsNullOrWhiteSpace(avatar))
        {
            byte[] data = ParseBase64(avatar);
            //TODO convert to png/jpeg format, calculate hash, and upload
            //TODO #2 implement website upload endpoint
            // this.ProfileUser.IconHash = "";
        }

        if (!this.ProfileUser.Biography.Equals(biography))
        {
            this.ProfileUser.Biography = SanitizationHelper.SanitizeString(biography);
        }
        //TODO set user timezone
        if (ServerConfiguration.Instance.Mail.MailEnabled)
        {
            //TODO 
            if (this.ProfileUser.EmailAddress != email)
            {
                if (await this.Database.Users.FirstOrDefaultAsync(u => u.EmailAddress != null && string.Equals(u.EmailAddress, email, StringComparison.CurrentCultureIgnoreCase)) == null)
                {
                    this.ProfileUser.EmailAddress = email;
                }
                else
                {
                    //TODO email is not verified
                }
            }
        }

        Console.WriteLine("avatar: " + avatar);
        Console.WriteLine("username: " + username);
        Console.WriteLine("email: " + email);
        Console.WriteLine("biography: " + biography);
        Console.WriteLine("timezone: " + timezone);

        return this.Redirect("~/user/" + userId);
    }

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);

        if(!this.User.IsModerator || this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        return this.Page();
    }
}