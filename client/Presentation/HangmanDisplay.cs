using System.Text;

namespace client.Presentation
{
    public class HangmanDisplay
    {
        private readonly HashSet<char> usedLetters = new HashSet<char>();
        private readonly string russianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        /// <summary>
        /// Отображает текущий этап виселицы на основе оставшихся попыток.
        /// </summary>
        public void DisplayHangman(int attemptsRemaining)
        {
            int stageIndex = HangmanStages.Stages.Length - attemptsRemaining - 1;
            if (stageIndex < 0) stageIndex = 0;
            if (stageIndex >= HangmanStages.Stages.Length) stageIndex = HangmanStages.Stages.Length - 1;

            Console.WriteLine(HangmanStages.Stages[stageIndex]);
        }

        /// <summary>
        /// Добавляет букву в список использованных и отображает обновленный алфавит.
        /// </summary>
        public void MarkLetterAsUsed(char letter)
        {
            usedLetters.Add(char.ToUpper(letter));
        }

        /// <summary>
        /// Отображает русский алфавит, перечеркивая использованные буквы.
        /// </summary>
        public void DisplayAlphabet()
        {
            var sb = new StringBuilder();

            foreach (var letter in russianAlphabet)
            {
                if (usedLetters.Contains(letter))
                {
                    sb.Append($"[{letter}]"); // Обозначим использованные буквы в скобках
                }
                else
                {
                    sb.Append($" {letter} ");
                }
            }

            Console.WriteLine("Available letters:");
            Console.WriteLine(sb.ToString());
        }
    }
}
