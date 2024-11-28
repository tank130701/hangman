package domain

import (
	"sync"
	"time"
)

type Room struct {
	ID           string
	Owner        *Player
	Players      []*Player
	LastActivity time.Time
	IsOpen       bool
	MaxPlayers   int
	Password     string
	StateManager *GameStateManager
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
