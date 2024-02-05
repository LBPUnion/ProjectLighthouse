using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class ServerConfiguration : ConfigurationBase<ServerConfiguration>
{
    // HEY, YOU!
    // THIS VALUE MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    //
    // This is so Lighthouse can properly identify outdated configurations and update them with newer settings accordingly.
    // If you are modifying anything here, this value MUST be incremented.
    // Thanks for listening~
    public override int ConfigVersion { get; set; } = 24;

    public override string ConfigName { get; set; } = "lighthouse.yml";
    public string WebsiteListenUrl { get; set; } = "http://localhost:10060";
    public string GameApiListenUrl { get; set; } = "http://localhost:10061";
    public string ApiListenUrl { get; set; } = "http://localhost:10062";

    public string DbConnectionString { get; set; } = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
    public string RedisConnectionString { get; set; } = "redis://localhost:6379";
    public string ExternalUrl { get; set; } = "http://localhost:10060";
    public string GameApiExternalUrl { get; set; } = "http://localhost:10060/LITTLEBIGPLANETPS3_XML";
    public string EulaText { get; set; } = "";
#if !DEBUG
    public string AnnounceText { get; set; } = "You are now logged in as %user.";
#else
    public string AnnounceText { get; set; } = "You are now logged in as %user (id: %id).";
#endif
    public bool CheckForUnsafeFiles { get; set; } = true;
    public bool LogChatFiltering { get; set; } = false;
    public bool LogChatMessages { get; set; } = false;

    public AuthenticationConfiguration Authentication { get; set; } = new();
    public CaptchaConfiguration Captcha { get; set; } = new();
    public DigestKeyConfiguration DigestKey { get; set; } = new();
    public GoogleAnalyticsConfiguration GoogleAnalytics { get; set; } = new();
    public MailConfiguration Mail { get; set; } = new();
    public UserGeneratedContentLimitConfiguration UserGeneratedContentLimits { get; set; } = new();
    public WebsiteConfiguration WebsiteConfiguration { get; set; } = new();
    public CustomizationConfiguration Customization { get; set; } = new();
    public RateLimitConfiguration RateLimitConfiguration { get; set; } = new();
    public TwoFactorConfiguration TwoFactorConfiguration { get; set; } = new();
    public RichPresenceConfiguration RichPresenceConfiguration { get; set; } = new();
    public NotificationConfiguration NotificationConfiguration { get; set; } = new();

    public override ConfigurationBase<ServerConfiguration> Deserialize(IDeserializer deserializer, string text) => deserializer.Deserialize<ServerConfiguration>(text);
}