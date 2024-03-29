namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class MailConfiguration
{
    public bool MailEnabled { get; set; }

    public string Host { get; set; } = "";

    public int Port { get; set; } = 587;

    public string FromAddress { get; set; } = "lighthouse@example.com";

    public string FromName { get; set; } = "Project Lighthouse";

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public bool UseSSL { get; set; } = true;
}