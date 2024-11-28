package domain

import (
	"errors"
	"time"
)

type GameState string

const (
	GameStateWaiting GameState = "WaitingForPlayers"
	GameStatePlaying GameState = "InProgress"
	GameStateOver    GameState = "GameOver"
)

type GameStateManager struct {
	state GameState
	game  *Game
}

func NewGameStateManager(word string, attempts int) *GameStateManager {
	return &GameStateManager{
		state: GameStateWaiting,
		game: &Game{
			Word:         word,
			GuessedWord:  make([]rune, len(word)),
			AttemptsLeft: attempts,
		},
	}
}

func (gsm *GameStateManager) GetState() GameState {
	return gsm.state
}

func (gsm *GameStateManager) SetState(state GameState) {
	gsm.state = state
}

func (gsm *GameStateManager) IsGameOver() bool {
	return gsm.state == GameStateOver
}

func (gsm *GameStateManager) StartGame() error {
	if gsm.state != GameStateWaiting {
		return errors.New("game cannot be started in the current state")
	}
	gsm.state = GameStatePlaying
	gsm.game.StartTime = time.Now()
	return nil
}

func (gsm *GameStateManager) MakeGuess(letter rune) (bool, string, error) {
	if gsm.state != GameStatePlaying {
		return false, "", errors.New("game is not in progress")
	}

	isCorrect := gsm.game.UpdateGuessedWord(letter)
	if isCorrect && gsm.game.IsWordGuessed() {
		gsm.state = GameStateOver
		return true, "Congratulations! You guessed the word: " + gsm.game.Word, nil
	}

	if !isCorrect {
		gsm.game.AttemptsLeft--
		if gsm.game.AttemptsLeft <= 0 {
			gsm.state = GameStateOver
			return false, "Game Over! The word was: " + gsm.game.Word, nil
		}
		return false, "Wrong guess!", nil
	}

	return true, "Correct guess!", nil
}
