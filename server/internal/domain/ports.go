package domain

type IRoomRepository interface {
	AddRoom(room *Room) error
	GetRoomByID(roomID string) (*Room, error)
	UpdateRoom(room *Room) error
	RemoveRoom(roomID string) error
	GetAllRooms() []*Room
}

type IRoomController interface {
	CreateRoom(player *Player, roomID, password string) (*Room, error)
	JoinRoom(player *Player, roomID, password string) (*Room, error)
	JoinRandomRoom(player *Player) (*Room, error)
	DeleteRoom(player *Player, roomID string) error
	LeaveRoom(player *Player, roomID string) error
	HandleOwnerChange(roomID string) error
	CleanupRooms(timeoutSeconds int)
	StartGame(player *Player, roomID string) error
	MakeGuess(player *Player, roomID string, letter rune) (bool, string, error)
	GetGameState(roomID string) (*GameState, error)
	GetAllRooms() ([]*Room, error)
}

type IPlayerRepository interface {
	AddPlayer(player *Player) error
	RemovePlayer(playerName string) error
	GetPlayerByName(playerName string) (*Player, error)
	GetAllPlayers() []*Player
	GetPlayerCount() int
}

type IGameService interface {
	StartGame(room *Room) error
	MakeGuess(room *Room, player *Player, letter rune) (bool, string, error)
	GetGameState(room *Room) (*GameState, error)
}
