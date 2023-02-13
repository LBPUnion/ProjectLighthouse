namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class PlayerCountObject
{
    public string Game { get; set; } = "";
    public int PlayerCount { get; set; }
}

public class PlayerCountResponse
{
    public int TotalPlayerCount { get; set; }
    public List<PlayerCountObject> Games { get; set; } = new();
}