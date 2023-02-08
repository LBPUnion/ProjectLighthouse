using System;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;

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

    public static string FilterMessage(string message)
    {
        if (CensorConfiguration.Instance.UserInputFilterMode == FilterMode.None) return message;

        int profaneIndex;

        foreach (string profanity in CensorConfiguration.Instance.FilteredWordList)
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

        sb.Append(message.AsSpan(0, profanityIndex));

        switch (CensorConfiguration.Instance.UserInputFilterMode)
        {
            case FilterMode.Random:
                char prevRandomChar = '\0';
                for (int i = 0; i < profanityLength; i++)
                {
                    if (message[i] == ' ')
                    {
                        sb.Append(' ');
                    }
                    else
                    {
                        char randomChar = randomCharacters[CryptoHelper.GenerateRandomInt32(0, randomCharacters.Length)];
                        if (randomChar == prevRandomChar) randomChar = randomCharacters[CryptoHelper.GenerateRandomInt32(0, randomCharacters.Length)];

                        prevRandomChar = randomChar;
                        sb.Append(randomChar);
                    }
                }

                break;
            case FilterMode.Asterisks:
                for(int i = 0; i < profanityLength; i++)
                {
                    sb.Append(message[i] == ' ' ? ' ' : '*');
                }

                break;
            case FilterMode.Furry:
                string randomWord = randomFurry[CryptoHelper.GenerateRandomInt32(0, randomFurry.Length)];
                sb.Append(randomWord);
                break;
            case FilterMode.None: break;
            default: throw new ArgumentOutOfRangeException(nameof(message));
        }

        sb.Append(message.AsSpan(profanityIndex + profanityLength));

        return sb.ToString();
    }

    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@')) return email;

        string[] emailArr = email.Split('@');
        string domainExt = Path.GetExtension(email);

        // Hides everything except the first and last character of the username and domain, preserves the domain extension (.net, .com)
        string maskedEmail = $"{emailArr[0][0]}****{emailArr[0][^1..]}@{emailArr[1][0]}****{emailArr[1]
            .Substring(emailArr[1].Length - domainExt.Length - 1,
                1)}{domainExt}";

        return maskedEmail;
    }
}