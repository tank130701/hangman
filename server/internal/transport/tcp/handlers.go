package tcp

import (
	"encoding/json"
	"hangman/internal/domain"
	"hangman/internal/errs"
	tcp_server "hangman/pkg/tcp-server"
	"net"
	"strings"
)

type Handler struct {
	RoomController domain.IRoomController
}

func NewHandler(controller domain.IRoomController) *Handler {
	return &Handler{
		RoomController: controller,
	}
}

func (h *Handler) InitRoutes(srv *tcp_server.Server) {
	srv.RegisterHandler("CREATE_ROOM", h.handleCreateRoomRequest)
	srv.RegisterHandler("START_GAME", h.handleStartGameRequest)
	srv.RegisterHandler("JOIN_ROOM", h.handleJoinRoomRequest)
	srv.RegisterHandler("DELETE_ROOM", h.handleDeleteRoomRequest)
	srv.RegisterHandler("GUESS_LETTER", h.handleGuessLetterRequest)
	srv.RegisterHandler("GET_GAME_STATE", h.handleGetGameStateRequest)
	srv.RegisterHandler("GET_ALL_ROOMS", h.handleGetAllRoomsRequest)
}

func (h *Handler) handleCreateRoomRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto CreateRoomRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid CREATE_ROOM payload")
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	room, err := h.RoomController.CreateRoom(player, dto.RoomID, dto.Password, dto.Difficulty)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}

	response := map[string]string{
		"message":  "Room has been created successfully",
		"room_id:": room.ID,
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleStartGameRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto StartGameRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid START_GAME payload")
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	err := h.RoomController.StartGame(player, dto.RoomID)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}

	response := map[string]string{"message": "Game started successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleJoinRoomRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto JoinRoomRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid JOIN_ROOM payload")
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	room, err := h.RoomController.JoinRoom(player, dto.RoomID, dto.Password)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	responseBytes, err := json.Marshal(room)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleDeleteRoomRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto DeleteRoomRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid DELETE_ROOM payload")
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	err := h.RoomController.DeleteRoom(player, dto.RoomID)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	response := map[string]string{"message": "Room deleted successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleGuessLetterRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto GuessLetterRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid GUESS_LETTER payload")
	}

	// Проверяем длину введённого символа
	if len(dto.Letter) != 1 {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid letter input. Please provide a single character.")
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	isCorrect, feedback, err := h.RoomController.MakeGuess(player, dto.RoomID, []rune(dto.Letter)[0])
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}

	// Формируем успешный ответ
	response := GuessLetterResponse{
		PlayerUsername: dto.PlayerUsername,
		IsCorrect:      isCorrect,
		Feedback:       feedback,
	}

	// Проверяем окончание игры
	if strings.Contains(feedback, "Game Over") || strings.Contains(feedback, "Congratulations") {
		response.GameOver = true
	} else {
		response.GameOver = false
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleGetGameStateRequest(conn net.Conn, message []byte) ([]byte, error) {
	var dto GetGameStateRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(errs.ErrCodeInvalidJSON, "Invalid GET_GAME_STATE payload")
	}

	// Получаем состояния игры для всех игроков
	playerGameStates, err := h.RoomController.GetGameState(dto.RoomID)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, err.Error())
	}

	// Преобразуем данные из map[string]*GameState в map[string]*PlayerGameStateDTO
	players := make(map[string]*PlayerGameStateDTO)
	for username, state := range playerGameStates {
		players[username] = &PlayerGameStateDTO{
			WordProgress: state.WordProgress,
			AttemptsLeft: state.AttemptsLeft,
			IsGameOver:   state.IsGameOver,
		}
	}

	// Формируем ответ
	response := GetGameStateResponse{
		Players: players,
	}

	// Сериализация ответа в JSON
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, "Failed to serialize game state")
	}

	return responseBytes, nil
}

func (h *Handler) handleGetAllRoomsRequest(conn net.Conn, message []byte) ([]byte, error) {
	rooms, err := h.RoomController.GetAllRooms()
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, "Failed to fetch rooms")
	}

	// Преобразуем комнаты в DTO для клиента
	roomDTOs := make([]map[string]interface{}, len(rooms))
	for i, room := range rooms {
		roomDTOs[i] = map[string]interface{}{
			"id":            room.ID,
			"owner":         room.Owner.Username,
			"players_count": room.PlayersRepo.GetPlayerCount(),
			"max_players":   room.MaxPlayers,
			"is_open":       room.IsOpen,
			"last_activity": room.LastActivity,
		}
	}

	responseBytes, err := json.Marshal(roomDTOs)
	if err != nil {
		return nil, errs.NewError(errs.ErrCodeInternalServerError, "Failed to serialize rooms")
	}

	return responseBytes, nil
}
