package service

import (
	"errors"
	"hangman/internal/domain"
)

type GameServiceImpl struct{}

func NewGameService() domain.IGameService {
	return &GameServiceImpl{}
}

// StartGame запускает игру
func (gs *GameServiceImpl) StartGame(room *domain.Room) error {
	room.Lock()
	defer room.Unlock()

	if room.StateManager == nil {
		room.StateManager = domain.NewGameStateManager("golang", 7)
	}

	return room.StateManager.StartGame()
}

// MakeGuess обрабатывает ход игрока
func (gs *GameServiceImpl) MakeGuess(room *domain.Room, player *domain.Player, letter rune) (bool, string, error) {
	room.Lock()
	defer room.Unlock()

	if room.StateManager == nil {
		return false, "", errors.New("no game in this room")
	}

	return room.StateManager.MakeGuess(letter)
}

// GetGameState возвращает текущее состояние игры
func (gs *GameServiceImpl) GetGameState(room *domain.Room) (*domain.GameState, error) {
	room.Lock()
	defer room.Unlock()

	if room.StateManager == nil {
		return nil, errors.New("no game in this room")
	}
	gameState := room.StateManager.GetState()

	return &gameState, nil
}
