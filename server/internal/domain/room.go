package domain

import (
	"context"
	"encoding/json"
	"fmt"
	tcp_server "hangman/pkg/tcp-server"
	"hangman/pkg/utils"
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
	ID                 string
	Owner              *string
	Players            map[string]*Player // Используем map для хранения игроков
	notificationServer *tcp_server.NotificationServer
	LastActivity       time.Time
	MaxPlayers         int
	Password           string
	Category           string
	Difficulty         string
	StateManager       *GameStateManager
	RoomState          roomState
	mu                 sync.RWMutex
}

// Конструктор для Room
func NewRoom(ctx context.Context, id string, owner *string, maxPlayers int, password, category, difficulty string) *Room {
	// Извлекаем логгер из контекста
	logger, ok := ctx.Value("logger").(tcp_server.ILogger)
	if !ok {
		logger = utils.NewCustomLogger(utils.LevelInfo)
	}
	//TODO: make ctxKeyPackage
	notificationSrv, ok := ctx.Value("notificationServer").(*tcp_server.NotificationServer)
	if !ok {
		logger.Error("Failed to load notification server")
	}
	return &Room{
		ID:                 id,
		Password:           password,
		Owner:              owner,
		Players:            make(map[string]*Player),
		LastActivity:       time.Now(),
		MaxPlayers:         maxPlayers,
		Category:           category,
		Difficulty:         difficulty,
		RoomState:          Waiting,
		notificationServer: notificationSrv,
	}
}

func (r *Room) MonitorContext(ctx context.Context, username string) {
	go func() {
		<-ctx.Done() // Ожидаем отмены контекста

		// Извлекаем логгер из контекста
		logger, ok := ctx.Value("logger").(tcp_server.ILogger) // Предполагаем, что у вас есть интерфейс Logger
		if ok {
			logger.Info(fmt.Sprintf("Player %s: context canceled, kicking from room", username))
		} else {
			fmt.Printf("Player %s: context canceled, kicking from room (logger not found)\n", username)
		}
		//time.Sleep(3 * time.Second)
		// Кикаем игрока
		r.KickPlayer(username)
	}()
}

func (r *Room) KickPlayer(username string) {
	r.Lock()
	defer r.Unlock()

	if player, exists := r.Players[username]; exists {
		// Извлекаем функцию отмены контекста
		cancel, ok := player.Ctx.Value("cancel").(context.CancelFunc)
		if ok {
			cancel() // Отменяем контекст
		}

		// Удаляем игрока из комнаты
		delete(r.Players, username)
	}
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

func (r *Room) NotifyPlayers(event string, payload interface{}) error {
	// Сериализация данных события
	message, err := json.Marshal(payload)
	if err != nil {
		return fmt.Errorf("failed to serialize event payload: %w", err)
	}

	// Получение списка подключенных клиентов
	clients := r.getConnectedClientsIPs()

	if len(clients) == 0 {
		return fmt.Errorf("no connected clients to notify")
	}

	// Отправка уведомлений
	r.notificationServer.Notify(event, message, clients)

	return nil
}

// Вспомогательный метод для получения списка подключенных клиентов
func (r *Room) getConnectedClientsIPs() []string {
	r.mu.Lock()
	defer r.mu.Unlock()

	var clientIPs []string
	for _, player := range r.Players {
		if player.IsConnected && player.Conn != nil {
			remoteAddr := (*player.Conn).RemoteAddr().String()
			// Извлекаем только IP-адрес из "IP:port"
			host, _, err := net.SplitHostPort(remoteAddr)
			if err == nil {
				clientIPs = append(clientIPs, host)
			} else {
				fmt.Printf("failed to parse remote address: %v\n", err)
			}
		}
	}
	return clientIPs
}
