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
        StringBuilder stringBuilder = new(message);
        if (CensorConfiguration.Instance.UserInputFilterMode == FilterMode.None) return message;

        int profaneIndex;
        int profaneCount = 0;

        string originalMessage = message;

        foreach (string profanity in CensorConfiguration.Instance.FilteredWordList)
            do
            {
                profaneIndex = message.IndexOf(profanity, StringComparison.OrdinalIgnoreCase);

                if (profaneIndex == -1) continue;

                Censor(profaneIndex, profanity.Length, stringBuilder);
                profaneCount += 1;
            }
            while (profaneIndex != -1);

        if (profaneCount > 0 && message.Length <= 94 && ServerConfiguration.Instance.LogChatFiltering) // 94 = lbp char limit
            Logger.Info($"Censored {profaneCount} profane words from message \"{originalMessage}\"", LogArea.Filter);

        return stringBuilder.ToString();
    }

    private static void Censor(int profanityIndex, int profanityLength, StringBuilder message)
    {
        switch (CensorConfiguration.Instance.UserInputFilterMode)
        {
            case FilterMode.Random:
                char prevRandomChar = '\0';
                for (int i = profanityIndex; i < profanityIndex + profanityLength; i++)
                {
                    if (char.IsWhiteSpace(message[i])) continue;
                    
                    char randomChar = randomCharacters[CryptoHelper.GenerateRandomInt32(0, randomCharacters.Length)];
                    if (randomChar == prevRandomChar)
                        randomChar = randomCharacters[CryptoHelper.GenerateRandomInt32(0, randomCharacters.Length)];

                    prevRandomChar = randomChar;
                    message[i] = randomChar;
                }

                break;
            case FilterMode.Asterisks:
                for(int i = profanityIndex; i < profanityIndex + profanityLength; i++)
                {
                    if (char.IsWhiteSpace(message[i])) continue;

                    message[i] = '*';
                }

                break;
            case FilterMode.Furry:
                string randomWord = randomFurry[CryptoHelper.GenerateRandomInt32(0, randomFurry.Length)];
                string afterProfanity = message.ToString(profanityIndex + profanityLength,
                    message.Length - (profanityIndex + profanityLength));

                message.Length = profanityIndex;

                message.Append(randomWord);
                message.Append(afterProfanity);
                break;
            case FilterMode.None: break;
            default: throw new ArgumentOutOfRangeException(nameof(message));
        }
    }
}