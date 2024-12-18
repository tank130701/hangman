package domain

type PlayerUsername string

type GameState struct {
	WordProgress string // Текущее состояние слова (с угаданными буквами)
	AttemptsLeft int    // Остаток попыток
	IsGameOver   bool   // Статус завершения игры
	Score        int    // Текущий счет игрока
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
		Score:        gsm.games[username].Score,
	}
}

func (gsm *GameStateManager) MakeGuess(username PlayerUsername, letter rune) (bool, string, error) {
	game := gsm.games[username]

	isCorrect := game.UpdateGuessedWord(letter)

	// Правильный ответ
	if isCorrect {
		game.Score += 10 // Начислить очки за правильную букву

		if game.IsWordGuessed() {
			game.Score += 50 // Бонус за завершение слова
			return true, "Congratulations! You guessed the word: " + game.Word, nil
		}

		return true, "Correct guess!", nil
	}

	// Неправильный ответ
	game.AttemptsLeft--
	game.Score -= 5 // Штраф за неправильный ответ
	if game.Score < 0 {
		game.Score = 0 // Убедиться, что счет не становится отрицательным
	}

	// Проверить, закончились ли попытки
	if game.AttemptsLeft <= 0 {
		return false, "Game Over! The word was: " + game.Word, nil
	}

	return false, "Wrong guess!", nil
}
