using System;
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;

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
        int profaneCount = 0;

        string originalMessage = message;

        foreach (string profanity in CensorConfiguration.Instance.FilteredWordList)
            do
            {
                profaneIndex = message.ToLower().IndexOf(profanity, StringComparison.Ordinal);

                if (profaneIndex == -1) continue;

                message = Censor(profaneIndex, profanity.Length, message);
                profaneCount += 1;
            }
            while (profaneIndex != -1);

        if (profaneCount > 0 && message.Length <= 94 && ServerConfiguration.Instance.LogChatFiltering) // 94 = lbp char limit
            Logger.Info($"Censored {profaneCount} profane words from message \"{originalMessage}\"", LogArea.Filter);

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
}