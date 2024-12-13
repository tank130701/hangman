package tcp

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

type PlayerGameState struct {
	WordProgress string `json:"word_progress"` // Прогресс текущего слова
	AttemptsLeft int    `json:"attempts_left"` // Остаток попыток
	IsGameOver   bool   `json:"is_game_over"`  // Указатель на завершение игры
}

type GetGameStateResponse struct {
	Players map[string]*PlayerGameState `json:"players"` // Карта состояний игроков
}
