#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Types.Settings.Legacy;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LBPUnion.ProjectLighthouse.Types.Settings;

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
    public const int CurrentConfigVersion = 1;

    #region Meta

    public static ServerConfiguration Instance;

    [YamlMember(Alias = "configVersionDoNotModifyOrYouWillBeSlapped")]
    public int ConfigVersion { get; set; } = CurrentConfigVersion;

    public const string ConfigFileName = "lighthouse.yml";
    public const string LegacyConfigFileName = LegacyServerSettings.ConfigFileName;

    #endregion Meta

    #region Setup

    private static FileSystemWatcher fileWatcher;

    // ReSharper disable once NotNullMemberIsNotInitialized
    #pragma warning disable CS8618
    static ServerConfiguration()
    {
        if (ServerStatics.IsUnitTesting) return; // Unit testing, we don't want to read configurations here since the tests will provide their own

        Logger.LogInfo("Loading config...", LogArea.Config);

        ServerConfiguration? tempConfig;

        // If a valid YML configuration is available!
        if (File.Exists(ConfigFileName) && (tempConfig = fromFile(ConfigFileName)) != null)
        {
//            Instance = JsonSerializer.Deserialize<ServerConfiguration>(configFile) ?? throw new ArgumentNullException(nameof(ConfigFileName));
            Instance = tempConfig;

            if (Instance.ConfigVersion < CurrentConfigVersion)
            {
                Logger.LogInfo($"Upgrading config file from version {Instance.ConfigVersion} to version {CurrentConfigVersion}", LogArea.Config);
                Instance.ConfigVersion = CurrentConfigVersion;

                Instance.writeConfig(ConfigFileName);
            }
        }
        // If we have a valid legacy configuration we can migrate, let's do it now.
        else if (File.Exists(LegacyConfigFileName))
        {
            Logger.LogWarn("This version of Project Lighthouse now uses YML instead of JSON to store configuration.", LogArea.Config);
            Logger.LogWarn
                ("As such, the config will now be migrated to use YML. Do not modify the original JSON file; changes will not be kept.", LogArea.Config);
            Logger.LogInfo($"The new configuration is stored at {ConfigFileName}.", LogArea.Config);

            LegacyServerSettings? legacyConfig = LegacyServerSettings.FromFile(LegacyConfigFileName);
            Debug.Assert(legacyConfig != null);
            Instance = legacyConfig.ToNewConfiguration();

            Instance.writeConfig(ConfigFileName);

            Logger.LogSuccess("The configuration migration completed successfully.", LogArea.Config);
        }
        // If there is no valid YML configuration available,
        // generate a blank one and ask the server operator to configure it, then exit.
        else
        {
            new ServerConfiguration().writeConfig(ConfigFileName + ".configme");

            Logger.LogWarn
            (
                "The configuration file was not found. " +
                "A blank configuration file has been created for you at " +
                $"{Path.Combine(Environment.CurrentDirectory, ConfigFileName + ".configme")}",
                LogArea.Config
            );

            Environment.Exit(1);
        }

        // Set up reloading
        if (Instance.ConfigReloading)
        {
            Logger.LogInfo("Setting up config reloading...", LogArea.Config);
            fileWatcher = new FileSystemWatcher
            {
                Path = Environment.CurrentDirectory,
                Filter = ConfigFileName,
                NotifyFilter = NotifyFilters.LastWrite, // only watch for writes to config file
            };

            fileWatcher.Changed += onConfigChanged; // add event handler

            fileWatcher.EnableRaisingEvents = true; // begin watching
        }
    }
    #pragma warning restore CS8618

    private static void onConfigChanged(object sender, FileSystemEventArgs e)
    {
        Debug.Assert(e.Name == ConfigFileName);
        Logger.LogInfo("Configuration file modified, reloading config...", LogArea.Config);
        Logger.LogWarn("Some changes may not apply; they will require a restart of Lighthouse.", LogArea.Config);

        ServerConfiguration? configuration = fromFile(ConfigFileName);
        if (configuration == null)
        {
            Logger.LogWarn("The new configuration was unable to be loaded for some reason. The old config has been kept.", LogArea.Config);
            return;
        }

        Instance = configuration;

        Logger.LogSuccess("Successfully reloaded the configuration!", LogArea.Config);
    }

    private static INamingConvention namingConvention = CamelCaseNamingConvention.Instance;

    private static ServerConfiguration? fromFile(string path)
    {
        IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(namingConvention).Build();

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

    public string ListenUrl { get; set; } = "http://localhost:10060";
    public string DbConnectionString { get; set; } = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
    public string ExternalUrl { get; set; } = "http://localhost:10060";
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

}