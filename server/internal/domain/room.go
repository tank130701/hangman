package domain

import (
	"encoding/json"
	"fmt"
	tcp_server "hangman/pkg/tcp-server"
	"net"
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
	Players      map[string]*Player // Используем map для хранения игроков
	LastActivity time.Time
	MaxPlayers   int
	Password     string
	Category     string
	Difficulty   string
	StateManager *GameStateManager
	RoomState    roomState
	mu           sync.RWMutex
}

func (r *Room) SetState(state roomState) {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.RoomState = state
}

// RoomLock блокирует комнату для потокобезопасной работы
func (r *Room) RLock() {
	r.mu.RLock()
}

// RoomUnlock снимает блокировку с комнаты
func (r *Room) RUnlock() {
	r.mu.RUnlock()
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

func (r *Room) AddPlayer(player *Player) {
	r.mu.Lock()
	defer r.mu.Unlock()

	if r.Players == nil {
		r.Players = make(map[string]*Player) // Инициализация Players, если nil
	}

	if len(r.Players) >= r.MaxPlayers {
		return // Не добавляем игрока, если комната заполнена
	}

	r.Players[player.Username] = player
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
	r.mu.Lock()
	defer r.mu.Unlock()

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

type GameStartedEvent struct {
	Event   string             `json:"event"`   // Тип события
	Payload GameStartedPayload `json:"payload"` // Данные события
}

type GameStartedPayload struct {
	Category   string `json:"category"`   // Категория игры
	Difficulty string `json:"difficulty"` // Сложность игры
}

func (r *Room) NotifyPlayers(event string, payload GameStartedPayload) error {
	// Сериализация данных события
	message, err := json.Marshal(payload)
	if err != nil {
		return fmt.Errorf("failed to serialize event: %w", err)
	}

	// Собираем список подключенных клиентов
	var clients []net.Conn

	r.mu.Lock()
	defer r.mu.Unlock()

	for _, player := range r.Players {
		if player.IsConnected && player.Username != *r.Owner {
			clients = append(clients, player.Conn)
		}
	}

	// Используем метод Notify для отправки сообщений
	tcp_server.Notify(event, message, clients)
	return nil
}
