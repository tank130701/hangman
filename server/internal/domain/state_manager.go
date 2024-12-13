package domain

type PlayerUsername string

type GameState struct {
	WordProgress string // Текущее состояние слова (с угаданными буквами)
	AttemptsLeft int    // Остаток попыток
	IsGameOver   bool   // Статус завершения игры
}

type GameStateManager struct {
	games map[PlayerUsername]*Game
}

func NewGameStateManager() *GameStateManager {
	return &GameStateManager{
		games: make(map[PlayerUsername]*Game),
	}
}

func (gsm *GameStateManager) AddGame(word string, username PlayerUsername, attempts int) {
	gsm.games[username] = NewGame(word, attempts)
}

func (gsm *GameStateManager) GetState(username PlayerUsername) GameState {
	return GameState{
		WordProgress: gsm.games[username].DisplayWord(),
		AttemptsLeft: gsm.games[username].AttemptsLeft,
		IsGameOver:   gsm.games[username].IsWordGuessed() || gsm.games[username].AttemptsLeft <= 0,
	}
}

func (gsm *GameStateManager) MakeGuess(username PlayerUsername, letter rune) (bool, string, error) {
	isCorrect := gsm.games[username].UpdateGuessedWord(letter)
	if isCorrect && gsm.games[username].IsWordGuessed() {
		return true, "Congratulations! You guessed the word: " + gsm.games[username].Word, nil
	}

	if !isCorrect {
		gsm.games[username].AttemptsLeft--
		if gsm.games[username].AttemptsLeft <= 0 {
			return false, "Game Over! The word was: " + gsm.games[username].Word, nil
		}
		return false, "Wrong guess!", nil
	}

	return true, "Correct guess!", nil
}
