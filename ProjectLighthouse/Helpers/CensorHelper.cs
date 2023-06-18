using System;
using System.Collections.Generic;
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
        StringBuilder stringBuilder = new(message);

        foreach (string profanity in CensorConfiguration.Instance.FilteredWordList)
        {
            int lastFoundProfanity = 0;
            int profaneIndex;
            List<int> censorIndices = new();
            do
            {
                profaneIndex = message.IndexOf(profanity, lastFoundProfanity, StringComparison.OrdinalIgnoreCase);
                if (profaneIndex == -1) continue;

                censorIndices.Add(profaneIndex);
                
                lastFoundProfanity = profaneIndex + profanity.Length;
            }
            while (profaneIndex != -1);

            for (int i = censorIndices.Count - 1; i >= 0; i--)
            {
                Censor(censorIndices[i], profanity.Length, stringBuilder);
            }
        }

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
                    while (randomChar == prevRandomChar)
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
                message.Remove(profanityIndex, profanityLength);
                message.Insert(profanityIndex, randomWord);
                break;
            case FilterMode.None: break;
            default: throw new ArgumentOutOfRangeException(nameof(message));
        }
    }
}