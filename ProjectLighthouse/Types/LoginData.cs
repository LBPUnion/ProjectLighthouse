#nullable enable
using System;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Types;

/// <summary>
///     The data sent from POST /LOGIN.
/// </summary>
public class LoginData
{

    public static readonly string UsernamePrefix = Encoding.ASCII.GetString
    (
        new byte[]
        {
            0x04, 0x00, 0x20,
        }
    );

    public string Username { get; set; } = null!;

    /// <summary>
    ///     Converts a X-I-5 Ticket into `LoginData`.
    ///     https://www.psdevwiki.com/ps3/X-I-5-Ticket
    /// </summary>
    public static LoginData? CreateFromString(string str)
    {
        str = str.Replace("\b", ""); // Remove backspace characters

        using MemoryStream ms = new(Encoding.ASCII.GetBytes(str));
        using BinaryReader reader = new(ms);

        if (!str.Contains(UsernamePrefix)) return null;

        LoginData loginData = new();

        reader.BaseStream.Position = str.IndexOf(UsernamePrefix, StringComparison.Ordinal) + UsernamePrefix.Length;
        loginData.Username = BinaryHelper.ReadString(reader).Replace("\0", string.Empty);

        return loginData;
    }
}