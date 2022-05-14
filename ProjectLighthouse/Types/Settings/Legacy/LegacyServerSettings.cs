using System.IO;
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

namespace LBPUnion.ProjectLighthouse.Types.Settings.Legacy;
#nullable enable

internal class LegacyServerSettings
{

    #region Meta

    public const string ConfigFileName = "lighthouse.config.json";

    #endregion

    #region InfluxDB

    public bool InfluxEnabled { get; set; }
    public bool InfluxLoggingEnabled { get; set; }
    public string InfluxOrg { get; set; } = "lighthouse";
    public string InfluxBucket { get; set; } = "lighthouse";
    public string InfluxToken { get; set; } = "";
    public string InfluxUrl { get; set; } = "http://localhost:8086";

    #endregion

    public string EulaText { get; set; } = "";

    #if !DEBUG
    public string AnnounceText { get; set; } = "You are now logged in as %user.";
    #else
    public string AnnounceText { get; set; } = "You are now logged in as %user (id: %id).";
    #endif

    public string DbConnectionString { get; set; } = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";

    public string ExternalUrl { get; set; } = "http://localhost:10060";
    public string ServerDigestKey { get; set; } = "";
    public string AlternateDigestKey { get; set; } = "";
    public bool UseExternalAuth { get; set; }

    public bool CheckForUnsafeFiles { get; set; } = true;

    public bool RegistrationEnabled { get; set; } = true;

    #region UGC Limits

    /// <summary>
    ///     The maximum amount of slots allowed on users' earth
    /// </summary>
    public int EntitledSlots { get; set; } = 50;

    public int ListsQuota { get; set; } = 50;

    public int PhotosQuota { get; set; } = 500;

    public bool ProfileCommentsEnabled { get; set; } = true;

    public bool LevelCommentsEnabled { get; set; } = true;

    public bool LevelReviewsEnabled { get; set; } = true;

    #endregion

    #region Google Analytics

    public bool GoogleAnalyticsEnabled { get; set; }

    public string GoogleAnalyticsId { get; set; } = "";

    #endregion

    public bool BlockDeniedUsers { get; set; } = true;

    public bool BooingEnabled { get; set; } = true;

    public FilterMode UserInputFilterMode { get; set; } = FilterMode.None;

    #region Discord Webhook

    public bool DiscordWebhookEnabled { get; set; }

    public string DiscordWebhookUrl { get; set; } = "";

    #endregion

    public bool ConfigReloading { get; set; } = true;

    public string MissingIconHash { get; set; } = "";

    #region HCaptcha

    public bool HCaptchaEnabled { get; set; }

    public string HCaptchaSiteKey { get; set; } = "";

    public string HCaptchaSecret { get; set; } = "";

    #endregion

    public string ServerListenUrl { get; set; } = "http://localhost:10060";

    public bool ConvertAssetsOnStartup { get; set; } = true;

    #region SMTP

    public bool SMTPEnabled { get; set; }

    public string SMTPHost { get; set; } = "";

    public int SMTPPort { get; set; } = 587;

    public string SMTPFromAddress { get; set; } = "lighthouse@example.com";

    public string SMTPFromName { get; set; } = "Project Lighthouse";

    public string SMTPPassword { get; set; } = "";

    public bool SMTPSsl { get; set; } = true;

    #endregion

    internal static LegacyServerSettings? FromFile(string path)
    {
        string data = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LegacyServerSettings>(data);
    }

    internal ServerConfiguration ToNewConfiguration()
    {
        ServerConfiguration configuration = new();
        configuration.ConfigReloading = this.ConfigReloading;
        configuration.AnnounceText = this.AnnounceText;
        configuration.EulaText = this.EulaText;
        configuration.ExternalUrl = this.ExternalUrl;
        configuration.DbConnectionString = this.DbConnectionString;
        configuration.CheckForUnsafeFiles = this.CheckForUnsafeFiles;
        configuration.UserInputFilterMode = this.UserInputFilterMode;

        // configuration categories
        configuration.InfluxDB = new InfluxDBConfiguration
        {
            InfluxEnabled = this.InfluxEnabled,
            LoggingEnabled = this.InfluxLoggingEnabled,
            Bucket = this.InfluxBucket,
            Organization = this.InfluxOrg,
            Token = this.InfluxToken,
            Url = InfluxUrl,
        };

        configuration.Authentication = new AuthenticationConfiguration
        {
            RegistrationEnabled = this.RegistrationEnabled,
            BlockDeniedUsers = this.BlockDeniedUsers,
            UseExternalAuth = this.UseExternalAuth,
        };

        configuration.Captcha = new CaptchaConfiguration
        {
            CaptchaEnabled = this.HCaptchaEnabled,
            SiteKey = this.HCaptchaSiteKey,
            Secret = this.HCaptchaSecret,
        };

        configuration.Mail = new MailConfiguration
        {
            MailEnabled = this.SMTPEnabled,
            Host = this.SMTPHost,
            Password = this.SMTPPassword,
            Port = this.SMTPPort,
            FromAddress = this.SMTPFromAddress,
            FromName = this.SMTPFromName,
            UseSSL = this.SMTPSsl,
        };

        configuration.DigestKey = new DigestKeyConfiguration
        {
            PrimaryDigestKey = this.ServerDigestKey,
            AlternateDigestKey = this.AlternateDigestKey,
        };

        configuration.DiscordIntegration = new DiscordIntegrationConfiguration
        {
            DiscordIntegrationEnabled = this.DiscordWebhookEnabled,
            Url = this.DiscordWebhookUrl,
        };

        configuration.GoogleAnalytics = new GoogleAnalyticsConfiguration
        {
            AnalyticsEnabled = this.GoogleAnalyticsEnabled,
            Id = this.GoogleAnalyticsId,
        };

        configuration.UserGeneratedContentLimits = new UserGeneratedContentLimitConfiguration
        {
            BooingEnabled = this.BooingEnabled,
            EntitledSlots = this.EntitledSlots,
            ListsQuota = this.ListsQuota,
            PhotosQuota = this.PhotosQuota,
            LevelCommentsEnabled = this.LevelCommentsEnabled,
            LevelReviewsEnabled = this.LevelReviewsEnabled,
            ProfileCommentsEnabled = this.ProfileCommentsEnabled,
        };

        configuration.WebsiteConfiguration = new WebsiteConfiguration
        {
            MissingIconHash = this.MissingIconHash,
            ConvertAssetsOnStartup = this.ConvertAssetsOnStartup,
        };

        return configuration;
    }
}