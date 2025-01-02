package events

type GameStartedEventPayload struct {
	Category   string `json:"category"`   // Категория игры
	Difficulty string `json:"difficulty"` // Сложность игры
}

type PlayerJoinedEventPayload struct {
	Username string `json:"username"` // Имя игрока
}

type PlayerLeftEventPayload struct {
	Username string `json:"username"` // Имя игрока
}

type RoomHasBeenDeletedEventPayload struct {
	RoomId string `json:"room_id"`
}

type RoomHasBeenUpdatedEventPayload struct {
	RoomId string `json:"room_id"`
}
