package tcp

type BaseRequest struct {
	Command string `json:"command"`
}

type StartGameDTO struct {
	Command string `json:"command"` 
	RoomID  string `json:"room_id"` 
}

type CreateRoomDTO struct {
	Command  string `json:"command"`
	RoomID   string `json:"room_id"`
	Password string `json:"password"`
}

type JoinRoomDTO struct {
	Command    string `json:"command"`
	RoomID     string `json:"room_id"`
	PlayerName string `json:"player_name"`
	Password   string `json:"password"`
}

type DeleteRoomDTO struct {
	Command string `json:"command"`
	RoomID  string `json:"room_id"`
}

type GuessLetterDTO struct {
	Command    string `json:"command"`
	RoomID     string `json:"room_id"`
	PlayerName string `json:"player_name"`
	Letter     string `json:"letter"`
}

type GetGameStateDTO struct {
	Command string `json:"command"`
	RoomID  string `json:"room_id"`
}
