package domain

import (
	"sync"
	"time"
)

type roomState string

const (
	Waiting    roomState = "WaitingForPlayers"
	InProgress roomState = "InProgress"
	GameOver   roomState = "GameOver"
)

type Room struct {
	ID           string
	Owner        *string
	Players      map[string]struct{} // Используем map для хранения игроков
	LastActivity time.Time
	IsOpen       bool
	MaxPlayers   int
	Password     string
	Category     string
	Difficulty   string
	StateManager *GameStateManager
	RoomState    roomState
	mu           sync.RWMutex
}

// RoomLock блокирует комнату для потокобезопасной работы
func (r *Room) Lock() {
	r.mu.Lock()
}

// RoomUnlock снимает блокировку с комнаты
func (r *Room) RUnlock() {
	r.mu.RUnlock()
}

// RoomLock блокирует комнату для потокобезопасной работы
func (r *Room) RLock() {
	r.mu.RLock()
}

// RoomUnlock снимает блокировку с комнаты
func (r *Room) Unlock() {
	r.mu.Unlock()
}

func (r *Room) UpdateActivity() {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.LastActivity = time.Now()
}

func (r *Room) AddPlayer(username string) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if r.Players == nil {
		r.Players = make(map[string]struct{}) // Инициализация Players, если nil
	}

	if len(r.Players) >= r.MaxPlayers {
		return // Не добавляем игрока, если комната заполнена
	}

	r.Players[username] = struct{}{}
}

func (r *Room) RemovePlayer(username string) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if r.Players != nil {
		delete(r.Players, username) // Удаление игрока из map
	}
}

func (r *Room) HasPlayer(username string) bool {
	r.mu.Lock()
	defer r.mu.Unlock()

	_, exists := r.Players[username]
	return exists
}

func (r *Room) GetPlayerCount() int {
	r.mu.RLock()
	defer r.mu.RUnlock()

	return len(r.Players)
}

func (r *Room) GetAllPlayers() []string {
	r.mu.RLock()
	defer r.mu.RUnlock()

	players := make([]string, 0, len(r.Players))
	for username := range r.Players {
		players = append(players, username)
	}
	return players
}
