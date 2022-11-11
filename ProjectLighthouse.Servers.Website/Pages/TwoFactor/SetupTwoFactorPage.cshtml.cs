#nullable enable
using System.Security.Cryptography;
using System.Web;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.TwoFactor;

public class SetupTwoFactorPage : BaseLayout
{
    public SetupTwoFactorPage(Database database) : base(database)
    { }

    public string QrCode { get; set; } = "";

    public string Error { get; set; } = "";

    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (user.IsTwoFactorSetup) return this.RedirectToPage(nameof(LandingPage));

        Console.WriteLine(user.IsTwoFactorSetup);

        // Don't regenerate the two factor secret if they accidentally refresh the page
        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret)) user.TwoFactorSecret = CryptoHelper.GenerateTotpSecret();

        this.QrCode = getQrCode(user);

        await this.Database.SaveChangesAsync();

        return this.Page();
    }

    private static string GenerateQrCode(string text, int pixelsPerModule, Color darkColor, Color lightColor, bool drawQuietZones)
    {
        QRCodeGenerator qrGenerator = new();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        int size = (qrCodeData.ModuleMatrix.Count - (drawQuietZones ? 0 : 8)) * pixelsPerModule;
        int offset = drawQuietZones ? 0 : 4 * pixelsPerModule;

        Image image = new Image<Rgba32>(size, size);
        Rgba32 dark = darkColor.ToPixel<Rgba32>();
        Rgba32 light = lightColor.ToPixel<Rgba32>();
        image.Mutate(c => c.ProcessPixelRowsAsVector4((span, value) =>
        {
            for (int x = 0; x < span.Length; x++)
            {
                int y = value.Y;
                int offsetX = x + offset;
                int offsetY = y + offset;

                bool module =
                    qrCodeData.ModuleMatrix[((offsetY + pixelsPerModule) / pixelsPerModule - 1)][
                        ((offsetX + pixelsPerModule) / pixelsPerModule - 1)];
                if (module)
                {
                    span[x].X = dark.R / 255f;
                    span[x].Y = dark.G / 255f;
                    span[x].Z = dark.B / 255f;
                    span[x].W = dark.A / 255f;
                }
                else
                {
                    span[x].X = light.R / 255f;
                    span[x].Y = light.G / 255f;
                    span[x].Z = light.B / 255f;
                    span[x].W = light.A / 255f;
                }
            }
        }));
        return image.ToBase64String(PngFormat.Instance);
    }

    private static string getQrCode(User user)
    {
        string instanceName = ServerConfiguration.Instance.Customization.ServerName;
        string totpLink = CryptoHelper.GenerateTotpLink(user.TwoFactorSecret, HttpUtility.HtmlEncode(instanceName), user.Username);
        return GenerateQrCode(totpLink, 6, Color.FromRgb(18, 18, 18), Color.Transparent, false);
    }

    public async Task<IActionResult> OnPost([FromForm] string? code)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (user.IsTwoFactorSetup) return this.RedirectToPage(nameof(LandingPage));

        if (CryptoHelper.verifyCode(code, user.TwoFactorSecret))
        {
            List<int> backups = new();
            for (int i = 0; i < 4; i++)
            {
                backups.Add(RandomNumberGenerator.GetInt32(100_000, 999_999));
            }
            user.TwoFactorBackup = string.Join(",", backups);

            await this.Database.SaveChangesAsync();

            return this.Page();
        }
        this.QrCode = getQrCode(user);
        this.Error = this.Translate(TwoFactorStrings.InvalidCode);

        return this.Page();
    }

}