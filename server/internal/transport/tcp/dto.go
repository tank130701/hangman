package tcp

import (
	"time"
)

type CreateRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
	Category       string `json:"category"`
	Difficulty     string `json:"difficulty"`
}

type CreateRoomResponse struct {
	Message string `json:"message"`
	RoomID  string `json:"room_id"`
}

type StartGameRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type StartGameResponse struct {
	Message string `json:"message"`
}

type JoinRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type PlayerDTO struct {
	Username    string `json:"username"`
	Score       int    `json:"score"`
	IsConnected bool   `json:"is_connected"`
}

type JoinRoomResponse struct {
	ID           string      `json:"id"`
	Owner        string      `json:"owner"`
	Players      []PlayerDTO `json:"players"`
	LastActivity time.Time   `json:"last_activity"`
	MaxPlayers   int         `json:"max_players"`
	Password     string      `json:"password"`
	Category     string      `json:"category"`
	Difficulty   string      `json:"difficulty"`
	RoomState    string      `json:"state"`
}

type GetRoomStateRequest struct {
	RoomID   string `json:"room_id"`
	Password string `json:"password"`
}
type GetRoomStateResponse struct {
	State   string      `json:"state"`
	Players []PlayerDTO `json:"players"`
}

type LeaveRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type LeaveRoomResponse struct {
	Message string `json:"message"`
	RoomID  string `json:"room_id"`
}

type DeleteRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type GuessLetterRequest struct {
	PlayerUsername string `json:"player_username"`
	Password       string `json:"password"`
	RoomID         string `json:"room_id"`
	Letter         string `json:"letter"`
}

type GuessLetterResponse struct {
	PlayerUsername string `json:"player_username"`
	IsCorrect      bool   `json:"is_correct"`
	GameOver       bool   `json:"game_over"`
	Feedback       string `json:"feedback"`
}

type GetGameStateRequest struct {
	PlayerUsername string `json:"player_username"`
	RoomID         string `json:"room_id"`
}

type PlayerGameStateDTO struct {
	WordProgress string `json:"word_progress"` // Прогресс текущего слова
	AttemptsLeft int    `json:"attempts_left"` // Остаток попыток
	IsGameOver   bool   `json:"is_game_over"`  // Указатель на завершение игры
	Score        int    `json:"score"`
}

type GetGameStateResponse struct {
	Players map[string]*PlayerGameStateDTO `json:"players"` // Карта состояний игроков
}

// RoomDTO описывает данные о комнате
type RoomDTO struct {
	ID           string `json:"id"`            // Уникальный идентификатор комнаты
	Owner        string `json:"owner"`         // Имя владельца комнаты
	PlayersCount int    `json:"players_count"` // Текущее количество игроков
	MaxPlayers   int    `json:"max_players"`   // Максимальное количество игроков
	GameState    string `json:"game_state"`
	//IsOpen       bool      `json:"is_open"`       // Статус комнаты (открыта/закрыта)
	LastActivity time.Time `json:"last_activity"` // Время последней активности
}

// GetAllRoomsResponse описывает ответ для запроса всех комнат
type GetAllRoomsResponse struct {
	Rooms []RoomDTO `json:"rooms"` // Список всех комнат
}

type PlayerScoreDTO struct {
	Username string `json:"username"`
	Score    int    `json:"score"`
}

type GetLeaderBoardResponse struct {
	Players []PlayerScoreDTO `json:"players"`
}
