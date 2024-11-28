package repository

import (
	"errors"
	"hangman/internal/domain"
	"sync"
)

type InMemoryPlayerRepository struct {
	players map[string]*domain.Player
	mu      sync.RWMutex
}

func NewPlayerRepository() domain.IPlayerRepository {
	return &InMemoryPlayerRepository{
		players: make(map[string]*domain.Player),
	}
}

func (r *InMemoryPlayerRepository) AddPlayer(player *domain.Player) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.players[player.Username]; exists {
		return errors.New("player already exists")
	}

	r.players[player.Username] = player
	return nil
}

func (r *InMemoryPlayerRepository) RemovePlayer(playerName string) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.players[playerName]; !exists {
		return errors.New("player not found")
	}

	delete(r.players, playerName)
	return nil
}

func (r *InMemoryPlayerRepository) GetPlayerByName(playerName string) (*domain.Player, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()

	player, exists := r.players[playerName]
	if !exists {
		return nil, errors.New("player not found")
	}

	return player, nil
}

func (r *InMemoryPlayerRepository) GetAllPlayers() []*domain.Player {
	r.mu.RLock()
	defer r.mu.RUnlock()

	players := make([]*domain.Player, 0, len(r.players))
	for _, player := range r.players {
		players = append(players, player)
	}

	return players
}
