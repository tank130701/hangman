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
	Owner        *Player
	PlayersRepo  IPlayerRepository
	LastActivity time.Time
	IsOpen       bool
	MaxPlayers   int
	Password     string
	StateManager *GameStateManager
	RoomState    roomState
	mu           sync.Mutex
}

// RoomLock блокирует комнату для потокобезопасной работы
func (r *Room) Lock() {
	r.mu.Lock()
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
