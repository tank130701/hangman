namespace HangmanClient.Domain.Entities
{
    public class Game
    {
        public string WordToGuess { get; private set; }
        public string DisplayedWord { get; private set; }
        public int RemainingAttempts { get; private set; }

        public Game(string wordToGuess, int maxAttempts)
        {
            WordToGuess = wordToGuess;
            DisplayedWord = new string('_', wordToGuess.Length);
            RemainingAttempts = maxAttempts;
        }

        public bool Guess(char letter)
        {
            bool correct = false;
            var chars = DisplayedWord.ToCharArray();

            for (int i = 0; i < WordToGuess.Length; i++)
            {
                if (WordToGuess[i] == letter)
                {
                    chars[i] = letter;
                    correct = true;
                }
            }

            DisplayedWord = new string(chars);

            if (!correct)
            {
                RemainingAttempts--;
            }

            return correct;
        }

        public bool IsGameOver()
        {
            return RemainingAttempts <= 0 || DisplayedWord == WordToGuess;
        }

        public bool HasWon()
        {
            return DisplayedWord == WordToGuess;
        }
    }
}
