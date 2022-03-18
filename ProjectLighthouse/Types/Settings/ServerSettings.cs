using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Types.Settings;

[Serializable]
public class ServerSettings
{
    public const int CurrentConfigVersion = 26; // MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    private static FileSystemWatcher fileWatcher;
    static ServerSettings()
    {
        if (ServerStatics.IsUnitTesting) return; // Unit testing, we don't want to read configurations here since the tests will provide their own

        Logger.Log("Loading config...", LoggerLevelConfig.Instance);

        if (File.Exists(ConfigFileName))
        {
            string configFile = File.ReadAllText(ConfigFileName);

            Instance = JsonSerializer.Deserialize<ServerSettings>(configFile) ?? throw new ArgumentNullException(nameof(ConfigFileName));

            if (Instance.ConfigVersion < CurrentConfigVersion)
            {
                Logger.Log($"Upgrading config file from version {Instance.ConfigVersion} to version {CurrentConfigVersion}", LoggerLevelConfig.Instance);
                Instance.ConfigVersion = CurrentConfigVersion;
                configFile = JsonSerializer.Serialize
                (
                    Instance,
                    typeof(ServerSettings),
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                    }
                );

                File.WriteAllText(ConfigFileName, configFile);
            }
        }
        else
        {
            string configFile = JsonSerializer.Serialize
            (
                new ServerSettings(),
                typeof(ServerSettings),
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                }
            );

            File.WriteAllText(ConfigFileName, configFile);

            Logger.Log
            (
                "The configuration file was not found. " +
                "A blank configuration file has been created for you at " +
                $"{Path.Combine(Environment.CurrentDirectory, ConfigFileName)}",
                LoggerLevelConfig.Instance
            );

            Environment.Exit(1);
        }

        // Set up reloading
        if (Instance.ConfigReloading)
        {
            Logger.Log("Setting up config reloading...", LoggerLevelConfig.Instance);
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

    private static void onConfigChanged(object sender, FileSystemEventArgs e)
    {
        Debug.Assert(e.Name == ConfigFileName);
        Logger.Log("Configuration file modified, reloading config.", LoggerLevelConfig.Instance);
        Logger.Log("Some changes may not apply, in which case may require a restart of Project Lighthouse.", LoggerLevelConfig.Instance);

        string configFile = File.ReadAllText(ConfigFileName);
        Instance = JsonSerializer.Deserialize<ServerSettings>(configFile) ?? throw new ArgumentNullException(nameof(ConfigFileName));
    }

    public bool InfluxEnabled { get; set; }
    public bool InfluxLoggingEnabled { get; set; }
    public string InfluxOrg { get; set; } = "lighthouse";
    public string InfluxBucket { get; set; } = "lighthouse";
    public string InfluxToken { get; set; } = "";
    public string InfluxUrl { get; set; } = "http://localhost:8086";

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

    /// <summary>
    ///     The maximum amount of slots allowed on users' earth
    /// </summary>
    public int EntitledSlots { get; set; } = 50;

    public int ListsQuota { get; set; } = 50;

    public int PhotosQuota { get; set; } = 500;

    public bool ProfileCommentsEnabled { get; set; } = true;

    public bool LevelCommentsEnabled { get; set; } = true;

    public bool LevelReviewsEnabled { get; set; } = true;

    public bool GoogleAnalyticsEnabled { get; set; }

    public string GoogleAnalyticsId { get; set; } = "";

    public bool BlockDeniedUsers { get; set; } = true;

    public bool BooingEnabled { get; set; } = true;

    public FilterMode UserInputFilterMode { get; set; } = FilterMode.None;

    public bool DiscordWebhookEnabled { get; set; }

    public string DiscordWebhookUrl { get; set; } = "";

    public bool ConfigReloading { get; set; } = true;

    public string MissingIconHash { get; set; } = "";

    public bool HCaptchaEnabled { get; set; }

    public string HCaptchaSiteKey { get; set; } = "";

    public string HCaptchaSecret { get; set; } = "";

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

    #region Meta

    [NotNull]
    public static ServerSettings Instance;

    [JsonPropertyName("ConfigVersionDoNotModifyOrYouWillBeSlapped")]
    public int ConfigVersion { get; set; } = CurrentConfigVersion;

    public const string ConfigFileName = "lighthouse.config.json";

    #endregion Meta

}