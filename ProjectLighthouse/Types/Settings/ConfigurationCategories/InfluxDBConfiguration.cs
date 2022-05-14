namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class InfluxDBConfiguration
{
    public bool InfluxEnabled { get; set; }

    /// <summary>
    /// Whether or not to log to InfluxDB.
    /// </summary>
    public bool LoggingEnabled { get; set; }

    public string Organization { get; set; } = "lighthouse";
    public string Bucket { get; set; } = "lighthouse";
    public string Token { get; set; } = "";
    public string Url { get; set; } = "http://localhost:8086";
}