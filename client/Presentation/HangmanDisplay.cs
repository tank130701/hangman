using System.Text;

namespace client.Presentation;

public class HangmanDisplay
{
    private readonly HashSet<char> usedLetters = new HashSet<char>();
    private readonly string russianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
    private readonly string[] hangmanStages = new string[]
    {
            "\n\n\n\n\n\n",
            "\n\n\n\n\n\n_____",
            "\n |\n |\n |\n |\n_|___",
            "_____\n |   |\n |\n |\n |\n_|___",
            "_____\n |   |\n |   O\n |\n |\n_|___",
            "_____\n |   |\n |   O\n |  /|\\\n |\n_|___",
            "_____\n |   |\n |   O\n |  /|\\\n |  / \\\n_|___"
    };

    /// <summary>
    /// Проверяет, содержится ли символ в русском алфавите.
    /// </summary>
    public bool Contains(char letter)
    {
        return russianAlphabet.Contains(char.ToUpper(letter));
    }

    /// <summary>
    /// Отображает текущий этап виселицы на основе оставшихся попыток.
    /// </summary>
    public void DisplayHangman(int attemptsRemaining)
    {
        int stageIndex = hangmanStages.Length - attemptsRemaining - 1;
        if (stageIndex < 0) stageIndex = 0;
        if (stageIndex >= hangmanStages.Length) stageIndex = hangmanStages.Length - 1;

        Console.WriteLine(hangmanStages[stageIndex]);
    }

    /// <summary>
    /// Добавляет букву в список использованных, если она является русским символом.
    /// </summary>
    public void MarkLetterAsUsed(char letter)
    {
        char upperLetter = char.ToUpper(letter);
        if (russianAlphabet.Contains(upperLetter))
        {
            usedLetters.Add(upperLetter);
        }
        else
        {
            Console.WriteLine($"Invalid character: {letter}. Only Russian letters are allowed.");
        }
    }

    /// <summary>
    /// Проверяет, была ли буква уже использована.
    /// </summary>
    public bool IsLetterUsed(char letter)
    {
        return usedLetters.Contains(char.ToUpper(letter));
    }

    /// <summary>
    /// Отображает оставшиеся буквы алфавита.
    /// </summary>
    public void DisplayAlphabet()
    {
        var sb = new StringBuilder();

        foreach (var letter in russianAlphabet)
        {
            if (!usedLetters.Contains(letter))
            {
                sb.Append(letter + " ");
            }
        }

        Console.WriteLine("Available letters:");
        Console.WriteLine(sb.ToString().Trim());
    }

    /// <summary>
    /// Помечает букву как использованную, если она еще не была использована.
    /// Возвращает true, если буква была успешно добавлена, и false, если она уже использована.
    /// </summary>
    public bool TryUseLetter(char letter)
    {
        char upperLetter = char.ToUpper(letter);

        if (IsLetterUsed(upperLetter))
        {
            Console.WriteLine($"The letter '{letter}' has already been used.");
            return false;
        }

        MarkLetterAsUsed(upperLetter);
        return true;
    }
}

