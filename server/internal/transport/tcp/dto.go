package tcp

type BaseRequest struct {
	Command string `json:"command"`
}

type CreateRoomRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
	Password       string `json:"password"`
}

type StartGameRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
}

type JoinRoomDTO struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
	PlayerName     string `json:"player_name"`
	Password       string `json:"password"`
}

type DeleteRoomDTO struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
}

type GuessLetterRequest struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
	PlayerName     string `json:"player_name"`
	Letter         string `json:"letter"`
}

type GuessLetterResponse struct {
	PlayerUsername string `json:"player_username"`
	IsCorrect      bool   `json:"is_correct"`
	GameOver       bool   `json:"game_over"`
	Feedback       string `json:"feedback"`
}
type GetGameStateDTO struct {
	PlayerUsername string `json:"player_username"`
	Command        string `json:"command"`
	RoomID         string `json:"room_id"`
}
