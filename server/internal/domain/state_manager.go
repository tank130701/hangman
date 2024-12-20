package domain

import (
	"fmt"
	"sync"
)

type PlayerUsername string

type GameState struct {
	WordProgress string // Текущее состояние слова (с угаданными буквами)
	AttemptsLeft int    // Остаток попыток
	IsGameOver   bool   // Статус завершения игры
	Score        int    // Текущий счет игрока
}

type GameStateManager struct {
	games map[PlayerUsername]*Game
	mu    sync.RWMutex // Для защиты карты игр
}

func NewGameStateManager() *GameStateManager {
	return &GameStateManager{
		games: make(map[PlayerUsername]*Game),
	}
}

func (gsm *GameStateManager) AddGame(word string, username PlayerUsername, attempts int) {
	gsm.mu.Lock()
	defer gsm.mu.Unlock()
	gsm.games[username] = NewGame(word, attempts)
}

func (gsm *GameStateManager) GetState(username PlayerUsername) (GameState, error) {
	gsm.mu.RLock()
	defer gsm.mu.RUnlock()
	game, exists := gsm.games[username]
	if !exists {
		return GameState{}, fmt.Errorf("no game found for username: %s", username)
	}
	return GameState{
		WordProgress: game.DisplayWord(),
		AttemptsLeft: game.AttemptsLeft,
		IsGameOver:   game.IsWordGuessed() || game.AttemptsLeft <= 0,
		Score:        game.Score,
	}, nil
}

func (gsm *GameStateManager) MakeGuess(player *Player, letter rune) (bool, string, error) {
	gsm.mu.RLock()
	defer gsm.mu.RUnlock()
	game, exists := gsm.games[PlayerUsername(player.Username)]
	if !exists {
		return false, "", fmt.Errorf("no game found for username: %s", player.Username)
	}

	isCorrect := game.UpdateGuessedWord(letter)

	// Правильный ответ
	if isCorrect {
		game.Score += 10 // Начислить очки за правильную букву
		player.Score += 10
		if game.IsWordGuessed() {
			game.Score += 50 // Бонус за завершение слова
			player.Score += 50
			return true, "Congratulations! You guessed the word: " + game.Word, nil
		}

		return true, "Correct guess!", nil
	}

	// Неправильный ответ
	game.AttemptsLeft--
	game.Score -= 5 // Штраф за неправильный ответ
	player.Score -= 5
	if game.Score < 0 {
		game.Score = 0 // Убедиться, что счет не становится отрицательным
	}

	// Проверить, закончились ли попытки
	if game.AttemptsLeft <= 0 {
		return false, "Game Over! The word was: " + game.Word, nil
	}

	return false, "Wrong guess!", nil
}
