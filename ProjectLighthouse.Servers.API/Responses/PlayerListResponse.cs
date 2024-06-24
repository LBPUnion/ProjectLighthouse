namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class PlayerListResponse
{
    public required List<PlayerListObject> Players { get; set; }
}

public class PlayerListObject
{
    public required string Username { get; set; }
    public required string Game { get; set; }
    public required string Platform { get; set; }
}
