package domain

import (
	"context"
	"time"
)

type IRoomRepository interface {
	AddRoom(room *Room) error
	GetRoomByID(roomID string) (*Room, error)
	UpdateRoom(room *Room) error
	RemoveRoom(roomID string) error
	GetAllRooms() []*Room
}

type IRoomController interface {
	CreateRoom(player string, roomID, password, category, difficulty string) (*Room, error)
	JoinRoom(ctx context.Context, username, roomID, password string) (*Room, error)
	StartGame(clientKey ClientKey, roomID string) error
	MakeGuess(clientKey ClientKey, roomID string, letter rune) (bool, string, error)
	GetRoomState(roomID, password string) (*string, error)
	DeleteRoom(clientKey ClientKey, roomID string) error
	LeaveRoom(clientKey ClientKey, roomID string) error
	HandleOwnerChange(room *Room) error
	CleanupRooms(timeoutSeconds int)
	GetGameState(roomID string) (map[string]*GameState, error)
	GetAllRooms() ([]*Room, error)
	GetLeaderboard() (map[string]int, error)
}

type IPlayerRepository interface {
	AddPlayer(key ClientKey, player *Player) error
	RemovePlayer(key ClientKey) error
	GetPlayerByKey(key ClientKey) (*Player, error)
	RemovePlayerByUsername(username string) error
	GetAllPlayers() []*Player
	GetPlayerCount() int
	MonitorConnections(timeout time.Duration, inactivePlayersChan chan<- []string)
	UpdatePlayerActivity(key ClientKey) error
	GetPlayerUsernamesAndScores() map[string]int
}

type IGameService interface {
	StartGame(room *Room) error
	MakeGuess(room *Room, player *Player, letter rune) (bool, string, error)
	GetGameState(room *Room) (map[string]*GameState, error)
}
