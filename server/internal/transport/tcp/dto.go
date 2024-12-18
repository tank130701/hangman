package tcp

import "time"

type CreateRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
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
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
}

type StartGameResponse struct {
	Message string `json:"message"`
}

type JoinRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type DeleteRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
}

type GuessLetterRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
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
	Command        string `json:"command"`
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
	ID           string    `json:"id"`            // Уникальный идентификатор комнаты
	Owner        string    `json:"owner"`         // Имя владельца комнаты
	PlayersCount int       `json:"players_count"` // Текущее количество игроков
	MaxPlayers   int       `json:"max_players"`   // Максимальное количество игроков
	IsOpen       bool      `json:"is_open"`       // Статус комнаты (открыта/закрыта)
	LastActivity time.Time `json:"last_activity"` // Время последней активности
}

// GetAllRoomsResponse описывает ответ для запроса всех комнат
type GetAllRoomsResponse struct {
	Rooms []RoomDTO `json:"rooms"` // Список всех комнат
}
