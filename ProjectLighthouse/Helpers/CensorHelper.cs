using System.Text;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers;

public enum FilterMode
{
    None,
    Asterisks,
    Random,
    Furry,
}

public static class CensorHelper
{
    private static readonly char[] _randomCharacters =
    {
        '!',
        '@',
        '#',
        '$',
        '&',
        '%',
        '-',
        'A',
        'b',
        'C',
        'd',
        'E',
        'f',
        'G',
        'h',
        'I',
        'j',
        'K',
        'l',
        'M',
        'n',
        'O',
        'p',
        'Q',
        'r',
        'S',
        't',
        'U',
        'v',
        'W',
        'x',
        'Y',
        'z',
    };

    private static readonly string[] _randomFurry =
    {
        "UwU",
        "OwO",
        "uwu",
        "owo",
        "o3o",
        ">.>",
        "*pounces on you*",
        "*boops*",
        "*baps*",
        ":P",
        "x3",
        "O_O",
        "xD",
        ":3",
        ";3",
        "^w^",
    };

    private static readonly string[] _censorList = ResourceHelper.readManifestFile("chatCensoredList.txt").Split("\n");

    public static string ScanMessage(string message)
    {
        if (ServerSettings.Instance.UserInputFilterMode == FilterMode.None) return message;

        int profaneIndex = -1;

        foreach (string profanity in _censorList)
            do
            {
                profaneIndex = message.ToLower().IndexOf(profanity);
                if (profaneIndex != -1)
                    message = Censor(profaneIndex,
                        profanity.Length,
                        message);
            }
            while (profaneIndex != -1);

        return message;
    }

    private static string Censor(int profanityIndex, int profanityLength, string message)
    {
        StringBuilder sb = new();

        string randomWord;
        char randomChar;
        char prevRandomChar = '\0';

        sb.Append(message.Substring(0,
            profanityIndex));

        switch (ServerSettings.Instance.UserInputFilterMode)
        {
            case FilterMode.Random:
                for (int i = 0; i < profanityLength; i++)
                    lock (RandomHelper.random)
                    {
                        randomChar = _randomCharacters[RandomHelper.random.Next(0,
                            _randomCharacters.Length - 1)];
                        if (randomChar == prevRandomChar)
                            randomChar = _randomCharacters[RandomHelper.random.Next(0,
                                _randomCharacters.Length - 1)];

                        prevRandomChar = randomChar;

                        sb.Append(randomChar);
                    }

                break;
            case FilterMode.Asterisks:
                for (int i = 0; i < profanityLength; i++) sb.Append('*');
                break;
            case FilterMode.Furry:
                lock (RandomHelper.random)
                {
                    randomWord = _randomFurry[RandomHelper.random.Next(0,
                        _randomFurry.Length - 1)];
                    sb.Append(randomWord);
                }

                break;
        }

        sb.Append(message.Substring(profanityIndex + profanityLength));

        return sb.ToString();
    }
}