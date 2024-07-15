using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

[JsonDerivedType(typeof(PlayerCountByGameObject))]
[JsonDerivedType(typeof(PlayerCountByPlatformObject))]
public class PlayerCountObject
{
    public int PlayerCount { get; set; }
}

public class PlayerCountByGameObject : PlayerCountObject
{
    public string Game { get; set; } = "";
}

public class PlayerCountByPlatformObject : PlayerCountObject
{
    public string Platform { get; set; } = "";
}

[JsonDerivedType(typeof(PlayerCountByGameResponse))]
[JsonDerivedType(typeof(PlayerCountByPlatformResponse))]
public class PlayerCountResponse
{
    public int TotalPlayerCount { get; set; }
}

public class PlayerCountByGameResponse : PlayerCountResponse
{
    public List<PlayerCountObject> Games { get; set; } = new();
}

public class PlayerCountByPlatformResponse : PlayerCountResponse
{
    public List<PlayerCountObject> Platforms { get; set; } = new();
}