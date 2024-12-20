package repository

import (
	"errors"
	"hangman/internal/domain"
	"io"
	"net"
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

	existingPlayer, exists := r.players[key]
	if exists {
		// Обновляем состояние существующего игрока
		existingPlayer.Conn = player.Conn
		existingPlayer.IsConnected = true
		existingPlayer.LastActive = time.Now()
		return nil // Возвращаем true если пользователь уже был в комнате
	}

	// Добавляем нового игрока
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
	r.mu.Lock()
	defer r.mu.Unlock()

	player, exists := r.players[key]
	if !exists {
		return nil, errors.New("player not found")
	}

	return player, nil
}

func (r *InMemoryPlayerRepository) GetAllPlayers() []*domain.Player {
	r.mu.Lock()
	defer r.mu.Unlock()

	players := make([]*domain.Player, 0, len(r.players))
	for _, player := range r.players {
		players = append(players, player)
	}

	return players
}

func (r *InMemoryPlayerRepository) GetPlayerCount() int {
	r.mu.Lock()
	defer r.mu.Unlock()

	return len(r.players)
}

func (r *InMemoryPlayerRepository) CheckConnections() []string {
	r.mu.Lock()
	defer r.mu.Unlock()

	var disconnectedPlayers []string

	for key, player := range r.players {
		buffer := make([]byte, 1)
		player.Conn.SetReadDeadline(time.Now().Add(10 * time.Millisecond))
		_, err := player.Conn.Read(buffer)
		if err != nil {
			if errors.Is(err, net.ErrClosed) || errors.Is(err, io.EOF) {
				// Добавляем имя отключенного игрока в список
				disconnectedPlayers = append(disconnectedPlayers, player.Username)
				delete(r.players, key)
			}
		} else {
			// Если соединение активно, обновляем время последней активности
			player.LastActive = time.Now()
		}
	}

	return disconnectedPlayers
}

func (r *InMemoryPlayerRepository) MonitorConnections(timeout time.Duration, inactivePlayersChan chan<- []string) {
	ticker := time.NewTicker(timeout) // Интервал проверки
	defer ticker.Stop()

	for range ticker.C {
		r.mu.Lock()
		var inactivePlayers []string
		for key, player := range r.players {
			if time.Since(player.LastActive) > 1*time.Minute {
				// Слишком долго без активности
				player.IsConnected = false
				inactivePlayers = append(inactivePlayers, player.Username)
				delete(r.players, key)
			}
		}
		r.mu.Unlock()

		// Передаем список неактивных игроков в канал
		if len(inactivePlayers) > 0 {
			inactivePlayersChan <- inactivePlayers
		}
	}
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
	r.mu.Lock()
	defer r.mu.Unlock()

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
	r.mu.Lock()
	defer r.mu.Unlock()

	userScores := make(map[string]int)
	for key, player := range r.players {
		userScores[key.Username] = player.Score
	}

	return userScores
}
