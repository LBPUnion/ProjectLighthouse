using System;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types;

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
    
    private static readonly string[] screamingInPain =
    {
        "AAAAAAAAAAAA", "AUGHHHHHHHHH", "ARGHHHHHHHHHHH",
    };
    
    private static readonly string[] halfLife =
    {
        "hello, freeman!", "FOR GODS SAKES, OPEN THE SILO DOORS!", "FREEMAN, YOU FOOL!", "oh, fiddlesticks. what now?",
        "my god, what are you doing?!", "oh dear.", "no...no! no! get it off me! get it off! get it off!", "good morning, gordon.",
        "please see the Half-Life: Uplink section.", "AAAUUUUGH (elevator)", "don't be a fool!", "LOOK GORDON, ROPES!",
        "why do we all have to wear these rediculous ties?",
    };

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
            case FilterMode.Scream:
                lock(CryptoHelper.Random)
                {
                    string randomWord = screamingInPain[CryptoHelper.Random.Next(0, screamingInPain.Length -1)];
                    sb.Append(randomWord);
                }
                
                break;
            case FilterMode.HalfLife:
                lock(CryptoHelper.Random)
                {
                    string randomWord = halfLife[CryptoHelper.Random.Next(0, halfLife.Length -1)];
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
