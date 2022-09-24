#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Configuration.Legacy;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LBPUnion.ProjectLighthouse.Configuration;

[Serializable]
public class ServerConfiguration
{
    // HEY, YOU!
    // THIS VALUE MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    //
    // This is so Lighthouse can properly identify outdated configurations and update them with newer settings accordingly.
    // If you are modifying anything here that isn't outside of a method, this value MUST be incremented.
    // It is also strongly recommended to not remove any items, else it will cause deserialization errors.
    // You can use an ObsoleteAttribute instead. Make sure you set it to error, though.
    //
    // Thanks for listening~
    public const int CurrentConfigVersion = 12;

    #region Meta

    public static ServerConfiguration Instance;

    [YamlMember(Alias = "configVersionDoNotModifyOrYouWillBeSlapped")]
    public int ConfigVersion { get; set; } = CurrentConfigVersion;

    public const string ConfigFileName = "lighthouse.yml";
    public const string LegacyConfigFileName = LegacyServerSettings.ConfigFileName;

    #endregion Meta

    #region Setup

    private static readonly FileSystemWatcher fileWatcher;

    // ReSharper disable once NotNullMemberIsNotInitialized
#pragma warning disable CS8618
    static ServerConfiguration()
    {
        if (ServerStatics.IsUnitTesting) return; // Unit testing, we don't want to read configurations here since the tests will provide their own

        Logger.Info("Loading config...", LogArea.Config);

        ServerConfiguration? tempConfig;

        // If a valid YML configuration is available!
        if (File.Exists(ConfigFileName) && (tempConfig = fromFile(ConfigFileName)) != null)
        {
            //            Instance = JsonSerializer.Deserialize<ServerConfiguration>(configFile) ?? throw new ArgumentNullException(nameof(ConfigFileName));
            Instance = tempConfig;

            if (Instance.ConfigVersion < CurrentConfigVersion)
            {
                Logger.Info($"Upgrading config file from version {Instance.ConfigVersion} to version {CurrentConfigVersion}", LogArea.Config);
                Instance.ConfigVersion = CurrentConfigVersion;

                Instance.writeConfig(ConfigFileName);
            }
        }
        // If we have a valid legacy configuration we can migrate, let's do it now.
        else if (File.Exists(LegacyConfigFileName))
        {
            Logger.Warn("This version of Project Lighthouse now uses YML instead of JSON to store configuration.", LogArea.Config);
            Logger.Warn
                ("As such, the config will now be migrated to use YML. Do not modify the original JSON file; changes will not be kept.", LogArea.Config);
            Logger.Info($"The new configuration is stored at {ConfigFileName}.", LogArea.Config);

            LegacyServerSettings? legacyConfig = LegacyServerSettings.FromFile(LegacyConfigFileName);
            Debug.Assert(legacyConfig != null);
            Instance = legacyConfig.ToNewConfiguration();

            Instance.writeConfig(ConfigFileName);

            Logger.Success("The configuration migration completed successfully.", LogArea.Config);
        }
        // If there is no valid YML configuration available,
        // generate a blank one and ask the server operator to configure it, then exit.
        else
        {
            new ServerConfiguration().writeConfig(ConfigFileName + ".configme");

            Logger.Warn
            (
                "The configuration file was not found. " +
                "A blank configuration file has been created for you at " +
                $"{Path.Combine(Environment.CurrentDirectory, ConfigFileName + ".configme")}",
                LogArea.Config
            );

            Environment.Exit(1);
        }

        // Set up reloading
        if (!Instance.ConfigReloading) return;

        Logger.Info("Setting up config reloading...", LogArea.Config);
        fileWatcher = new FileSystemWatcher
        {
            Path = Environment.CurrentDirectory,
            Filter = ConfigFileName,
            NotifyFilter = NotifyFilters.LastWrite, // only watch for writes to config file
        };

        fileWatcher.Changed += onConfigChanged; // add event handler

        fileWatcher.EnableRaisingEvents = true; // begin watching
    }
#pragma warning restore CS8618

    private static void onConfigChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            fileWatcher.EnableRaisingEvents = false;
            Debug.Assert(e.Name == ConfigFileName);
            Logger.Info("Configuration file modified, reloading config...", LogArea.Config);
            Logger.Warn("Some changes may not apply; they will require a restart of Lighthouse.", LogArea.Config);

            ServerConfiguration? configuration = fromFile(ConfigFileName);
            if (configuration == null)
            {
                Logger.Warn("The new configuration was unable to be loaded for some reason. The old config has been kept.", LogArea.Config);
                return;
            }

            Instance = configuration;

            Logger.Success("Successfully reloaded the configuration!", LogArea.Config);
        }
        finally
        {
            fileWatcher.EnableRaisingEvents = true;
        }
    }

    private static INamingConvention namingConvention = CamelCaseNamingConvention.Instance;

    private static ServerConfiguration? fromFile(string path)
    {
        IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(namingConvention).IgnoreUnmatchedProperties().Build();

        string text;

        try
        {
            text = File.ReadAllText(path);
        }
        catch
        {
            return null;
        }

        return deserializer.Deserialize<ServerConfiguration>(text);
    }

    private void writeConfig(string path)
    {
        ISerializer serializer = new SerializerBuilder().WithNamingConvention(namingConvention).Build();

        File.WriteAllText(path, serializer.Serialize(this));
    }

    #endregion

    public string WebsiteListenUrl { get; set; } = "http://localhost:10060";
    public string GameApiListenUrl { get; set; } = "http://localhost:10061";
    public string ApiListenUrl { get; set; } = "http://localhost:10062";

    public string DbConnectionString { get; set; } = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
    public string RedisConnectionString { get; set; } = "redis://localhost:6379";
    public string ExternalUrl { get; set; } = "http://localhost:10060";
    public string GameApiExternalUrl { get; set; } = "http://localhost:10060/LITTLEBIGPLANETPS3_XML";
    public bool ConfigReloading { get; set; }
    public string EulaText { get; set; } = "";
#if !DEBUG
    public string AnnounceText { get; set; } = "You are now logged in as %user.";
#else
    public string AnnounceText { get; set; } = "You are now logged in as %user (id: %id).";
#endif
    public bool CheckForUnsafeFiles { get; set; } = true;

    public FilterMode UserInputFilterMode { get; set; } = FilterMode.None;

    public AuthenticationConfiguration Authentication { get; set; } = new();
    public CaptchaConfiguration Captcha { get; set; } = new();
    public DigestKeyConfiguration DigestKey { get; set; } = new();
    public DiscordIntegrationConfiguration DiscordIntegration { get; set; } = new();
    public GoogleAnalyticsConfiguration GoogleAnalytics { get; set; } = new();
    public InfluxDBConfiguration InfluxDB { get; set; } = new();
    public MailConfiguration Mail { get; set; } = new();
    public UserGeneratedContentLimitConfiguration UserGeneratedContentLimits { get; set; } = new();
    public WebsiteConfiguration WebsiteConfiguration { get; set; } = new();
    public CustomizationConfiguration Customization { get; set; } = new();
    public RateLimitConfiguration RateLimitConfiguration { get; set; } = new();
}