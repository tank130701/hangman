package tcp

type GameStartedEventPayload struct {
	Category   string `json:"category"`   // Категория игры
	Difficulty string `json:"difficulty"` // Сложность игры
}
