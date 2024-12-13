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
		room.StateManager = domain.NewGameStateManager()
	}

	for _, player := range room.PlayersRepo.GetAllPlayers() {
		room.StateManager.AddGame("golang", domain.PlayerUsername(player.Username), 7)
	}

	return nil
}

// MakeGuess обрабатывает ход игрока
func (gs *GameServiceImpl) MakeGuess(room *domain.Room, player *domain.Player, letter rune) (bool, string, error) {
	room.Lock()
	defer room.Unlock()

	if room.StateManager == nil {
		return false, "", errors.New("no game in this room")
	}

	return room.StateManager.MakeGuess(domain.PlayerUsername(player.Username), letter)
}

// GetGameState возвращает текущее состояние игры для всех игроков в комнате
func (gs *GameServiceImpl) GetGameState(room *domain.Room) (map[string]*domain.GameState, error) {
	room.Lock()
	defer room.Unlock()

	if room.StateManager == nil {
		return nil, errors.New("no game in this room")
	}

	// Создаём карту для хранения состояния игры каждого игрока
	playerGameStates := make(map[string]*domain.GameState)

	// Получаем состояние для каждого игрока в комнате
	for _, player := range room.PlayersRepo.GetAllPlayers() {
		gameState := room.StateManager.GetState(domain.PlayerUsername(player.Username))
		playerGameStates[player.Username] = &gameState
	}

	return playerGameStates, nil
}
