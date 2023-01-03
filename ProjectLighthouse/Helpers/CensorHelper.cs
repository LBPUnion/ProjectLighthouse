using System;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class CensorHelper
{
    private static readonly char[] randomCharacters =
    {
        '!', '@', '#', '$', '&', '%', '-', '_',
    };

    private static readonly string[] randomFurry =
    {
        "UwU", "OwO", "uwu", "owo", "o3o", ">.>", "*pounces on you*", "*boops*", "*baps*", ":P", "x3", "O_O", "xD", ":3", ";3", "^w^",
    };

    private static readonly string[] defaultCensoredList =
    {
        "cunt", "fag", "faggot", "horny", "kook", "kys", "loli", "nigga", "nigger", "penis", "pussy", "retard", "retarded", "vagina", "vore", "restitched", "h4h"
    }

    public const string ConfigFileName = "chatCensoredList.txt";

    static CensorHelper()
    {

    CensorHelper? tempConfig;

    if (File.Exists(ConfigFileName) && (tempConfig = fromFile(ConfigFileName)) != null) 
    {
        Instance = tempConfig;
    }

    else
    {
        new CensorHelper().writeConfig(ConfigFileName + ".configme");

        Logger.Warn
        (
            "The chat censor list was not found. " +
            "A default configuration file has been created for you at " +
            $"{Path.Combine(Environment.CurrentDirectory, ConfigFileName + ".configme")}",
            LogArea.Config
        );

        Environment.Exit(1);
    }
    
    private static INamingConvention namingConvention = CamelCaseNamingConvention.Instance;

    private static CensorHelper? fromFile(string path)
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

        return deserializer.Deserialize<CensorHelper>(text);
    }

    private void writeConfig(string path)
    {
        ISerializer serializer = new SerializerBuilder().WithNamingConvention(namingConvention).Build();

        File.WriteAllText(path, serializer.Serialize(defaultCensoredList));
    }

    }

    private static readonly string[] censorList = ResourceHelper.ReadManifestFile("chatCensoredList.txt").Replace("\r", "").Split("\n");

    public static string ScanMessage(string message)
    {
        if (ServerConfiguration.Instance.UserInputFilterMode == FilterMode.None) return message;

        int profaneIndex = -1;

        foreach (string profanity in censorList)
            do
            {
                profaneIndex = message.ToLower().IndexOf(profanity, StringComparison.Ordinal);
                if (profaneIndex != -1) message = Censor(profaneIndex, profanity.Length, message);
            }
            while (profaneIndex != -1);

        return message;
    }

    private static string Censor(int profanityIndex, int profanityLength, string message)
    {
        StringBuilder sb = new();

        char prevRandomChar = '\0';

        sb.Append(message.AsSpan(0, profanityIndex));

        switch (ServerConfiguration.Instance.UserInputFilterMode)
        {
            case FilterMode.Random:
                for(int i = 0; i < profanityLength; i++)
                    lock(CryptoHelper.Random)
                    {
                        if (message[i] == ' ')
                        {
                            sb.Append(' ');
                        }
                        else
                        {
                            char randomChar = randomCharacters[CryptoHelper.Random.Next(0, randomCharacters.Length - 1)];
                            if (randomChar == prevRandomChar) randomChar = randomCharacters[CryptoHelper.Random.Next(0, randomCharacters.Length - 1)];

                            prevRandomChar = randomChar;

                            sb.Append(randomChar);
                        }
                    }

                break;
            case FilterMode.Asterisks:
                for(int i = 0; i < profanityLength; i++)
                {
                    if (message[i] == ' ')
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        sb.Append('*');
                    }
                }

                break;
            case FilterMode.Furry:
                lock(CryptoHelper.Random)
                {
                    string randomWord = randomFurry[CryptoHelper.Random.Next(0, randomFurry.Length - 1)];
                    sb.Append(randomWord);
                }

                break;
        }

        sb.Append(message.AsSpan(profanityIndex + profanityLength));

        return sb.ToString();
    }

    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return email;

        string[] emailArr = email.Split('@');
        string domainExt = Path.GetExtension(email);

        string maskedEmail = string.Format("{0}****{1}@{2}****{3}{4}",
            emailArr[0][0],
            emailArr[0].Substring(emailArr[0].Length - 1),
            emailArr[1][0],
            emailArr[1]
                .Substring(emailArr[1].Length - domainExt.Length - 1,
                    1),
            domainExt);

        return maskedEmail;
    }
}