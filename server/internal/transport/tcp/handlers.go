package tcp

import (
	"context"
	"encoding/json"
	"hangman/internal/domain"
	"hangman/internal/errs"
	tcp_server "hangman/pkg/tcp-server"
	"strings"
	"unicode/utf8"
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
	srv.RegisterHandler("LEAVE_ROOM", h.handleLeaveRoomRequest)
	srv.RegisterHandler("DELETE_ROOM", h.handleDeleteRoomRequest)
	srv.RegisterHandler("GUESS_LETTER", h.handleGuessLetterRequest)
	srv.RegisterHandler("GET_GAME_STATE", h.handleGetGameStateRequest)
	srv.RegisterHandler("GET_ALL_ROOMS", h.handleGetAllRoomsRequest)
	srv.RegisterHandler("GET_LEADERBOARD", h.handleGetLeaderBoard)
	srv.RegisterHandler("GET_ROOM_STATE", h.handleGetRoomStateRequest)
}

func (h *Handler) handleCreateRoomRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req CreateRoomRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid CREATE_ROOM payload")
	}

	room, err := h.RoomController.CreateRoom(req.PlayerUsername, req.RoomID, req.Password, req.Category, req.Difficulty)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	response := CreateRoomResponse{
		Message: "Room has been created successfully",
		RoomID:  room.ID,
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleJoinRoomRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req JoinRoomRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid JOIN_ROOM payload")
	}
	room, err := h.RoomController.JoinRoom(ctx, req.PlayerUsername, req.RoomID, req.Password)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	players := ConvertPlayersToSlice(room.Players)
	response := JoinRoomResponse{
		ID:           room.ID,
		Owner:        *room.Owner,
		Players:      players,
		LastActivity: room.LastActivity,
		MaxPlayers:   room.MaxPlayers,
		Password:     room.Password,
		Category:     room.Category,
		Difficulty:   room.Difficulty,
		RoomState:    string(room.RoomState),
	}
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleLeaveRoomRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req LeaveRoomRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid LEAVE_ROOM payload")
	}
	clientKey := domain.NewClientKey(req.PlayerUsername, req.Password)

	err := h.RoomController.LeaveRoom(clientKey, req.RoomID)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	response := LeaveRoomResponse{
		Message: "Successfully left the room",
		RoomID:  req.RoomID,
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleStartGameRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req StartGameRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid START_GAME payload")
	}
	clientKey := domain.NewClientKey(req.PlayerUsername, req.Password)
	err := h.RoomController.StartGame(clientKey, req.RoomID)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	response := StartGameResponse{Message: "Game started successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleDeleteRoomRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req DeleteRoomRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid DELETE_ROOM payload")
	}
	clientKey := domain.NewClientKey(req.PlayerUsername, req.Password)
	err := h.RoomController.DeleteRoom(clientKey, req.RoomID)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	response := map[string]string{"message": "Room deleted successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleGuessLetterRequest(ctx context.Context, message []byte) ([]byte, error) {
	var req GuessLetterRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid GUESS_LETTER payload")
	}

	// Проверяем длину введённого символа
	if utf8.RuneCountInString(req.Letter) != 1 {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid letter input. Please provide a single character.")
	}
	clientKey := domain.NewClientKey(req.PlayerUsername, req.Password)
	isCorrect, feedback, err := h.RoomController.MakeGuess(clientKey, req.RoomID, []rune(req.Letter)[0])
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	// Формируем успешный ответ
	response := GuessLetterResponse{
		PlayerUsername: req.PlayerUsername,
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
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}
	return responseBytes, nil
}

func (h *Handler) handleGetGameStateRequest(ctx context.Context, message []byte) ([]byte, error) {
	var dto GetGameStateRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid GET_GAME_STATE payload")
	}

	// Получаем состояния игры для всех игроков
	playerGameStates, err := h.RoomController.GetGameState(dto.RoomID)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	// Преобразуем данные из map[string]*GameState в map[string]*PlayerGameStateDTO
	players := make(map[string]*PlayerGameStateDTO)
	for username, state := range playerGameStates {
		players[username] = &PlayerGameStateDTO{
			WordProgress: state.WordProgress,
			AttemptsLeft: state.AttemptsLeft,
			IsGameOver:   state.IsGameOver,
			Score:        state.Score,
		}
	}

	// Формируем ответ
	response := GetGameStateResponse{
		Players: players,
	}

	// Сериализация ответа в JSON
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to serialize game state")
	}

	return responseBytes, nil
}

func (h *Handler) handleGetRoomStateRequest(ctx context.Context, message []byte) ([]byte, error) {
	// Разбираем запрос в DTO
	var req GetRoomStateRequest
	if err := json.Unmarshal(message, &req); err != nil {
		return nil, errs.NewError(tcp_server.StatusBadRequest, "Invalid GET_ROOM_STATE payload")
	}

	// Получаем состояние комнаты через контроллер
	roomState, err := h.RoomController.GetRoomState(req.RoomID, req.Password)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, err.Error())
	}

	// Формируем ответ DTO
	response := GetRoomStateResponse{
		State: *roomState,
	}

	// Сериализуем ответ в JSON
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to serialize response")
	}

	return responseBytes, nil
}

func (h *Handler) handleGetAllRoomsRequest(ctx context.Context, message []byte) ([]byte, error) {
	rooms, err := h.RoomController.GetAllRooms()
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to fetch rooms")
	}

	roomDTOs := make([]RoomDTO, len(rooms))
	for i, room := range rooms {
		roomDTOs[i] = RoomDTO{
			ID:           room.ID,
			Owner:        *room.Owner,
			PlayersCount: room.GetPlayerCount(),
			MaxPlayers:   room.MaxPlayers,
			LastActivity: room.LastActivity,
			GameState:    string(room.RoomState),
		}
	}

	response := GetAllRoomsResponse{
		Rooms: roomDTOs,
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to serialize rooms")
	}

	return responseBytes, nil
}

func (h *Handler) handleGetLeaderBoard(ctx context.Context, message []byte) ([]byte, error) {
	// Получаем данные о пользователях и их очках
	userScores, err := h.RoomController.GetLeaderboard()
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to fetch Leaderboard")
	}
	// Формируем DTO
	playerDTOs := make([]PlayerScoreDTO, 0, len(userScores))
	for username, score := range userScores {
		playerDTOs = append(playerDTOs, PlayerScoreDTO{
			Username: username,
			Score:    score,
		})
	}

	// Формируем ответ
	response := GetLeaderBoardResponse{
		Players: playerDTOs,
	}

	// Сериализуем в JSON
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return nil, errs.NewError(tcp_server.StatusInternalServerError, "Failed to serialize players scores")
	}

	return responseBytes, nil
}
