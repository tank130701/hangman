package repository

import (
	"errors"
	"hangman/internal/domain"
	"sync"
	"time"
)

type InMemoryPlayerRepository struct {
	players map[domain.ClientKey]*domain.Player
	mu      sync.RWMutex
}

func NewPlayerRepository() *InMemoryPlayerRepository {
	return &InMemoryPlayerRepository{
		players: make(map[domain.ClientKey]*domain.Player),
	}
}

func (r *InMemoryPlayerRepository) AddPlayer(key domain.ClientKey, player *domain.Player) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.players[key]; exists {
		return errors.New("player already exists")
	}

	r.players[key] = player
	return nil
}

func (r *InMemoryPlayerRepository) RemovePlayer(key domain.ClientKey) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.players[key]; !exists {
		return errors.New("player not found")
	}

	delete(r.players, key)
	return nil
}

func (r *InMemoryPlayerRepository) GetPlayerByKey(key domain.ClientKey) (*domain.Player, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()

	player, exists := r.players[key]
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

func (r *InMemoryPlayerRepository) GetPlayerCount() int {
	r.mu.RLock()
	defer r.mu.RUnlock()

	return len(r.players)
}

func (r *InMemoryPlayerRepository) CheckConnections(timeout time.Duration) []domain.ClientKey {
	now := time.Now()
	var toRemove []domain.ClientKey // Список игроков для удаления

	r.mu.Lock()
	defer r.mu.Unlock()

	for key, player := range r.players {
		if player.IsConnected {
			// Отправляем пинг игроку
			if _, err := player.Conn.Write([]byte("ping")); err != nil {
				// Если соединение разорвано, помечаем как отключённого
				player.IsConnected = false
				player.LastActive = now
			}
		} else {
			// Проверяем, истёк ли таймаут на реконнект
			if now.Sub(player.LastActive) > timeout {
				toRemove = append(toRemove, key)
			}
		}
	}

	// Удаляем игроков, которые не реконнектнулись за timeout
	for _, key := range toRemove {
		player := r.players[key]
		player.Conn.Close()
		delete(r.players, key)
	}

	return toRemove
}

func (r *InMemoryPlayerRepository) UpdatePlayerActivity(key domain.ClientKey) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	player, exists := r.players[key]
	if !exists {
		return errors.New("player not found")
	}

	player.IsConnected = true
	player.LastActive = time.Now()
	return nil
}

func (r *InMemoryPlayerRepository) GetPlayerByUsername(username string) (*domain.Player, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()

	for key, player := range r.players {
		if key.Username == username {
			return player, nil
		}
	}

	return nil, errors.New("player not found")
}

func (r *InMemoryPlayerRepository) RemovePlayerByUsername(username string) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	for key := range r.players {
		if key.Username == username {
			delete(r.players, key)
			return nil
		}
	}

	return errors.New("player not found")
}

func (r *InMemoryPlayerRepository) GetPlayerUsernamesAndScores() map[string]int {
	r.mu.RLock()
	defer r.mu.RUnlock()

	userScores := make(map[string]int)
	for key, player := range r.players {
		userScores[key.Username] = player.Score
	}

	return userScores
}
