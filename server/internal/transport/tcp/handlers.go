package tcp

import (
	"encoding/json"
	"fmt"
	"hangman/internal/domain"
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

func (h *Handler) HandleCreateRoomRequest(message []byte, conn net.Conn) ([]byte, bool) {
	var dto CreateRoomRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid CREATE_ROOM payload"), false
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	room, err := h.RoomController.CreateRoom(player, dto.RoomID, dto.Password)
	if err != nil {
		return createErrorResponse(5000, err.Error()), false
	}

	response := map[string]string{"message": fmt.Sprintf("Room has been created successfully (room_id:  %s)", room.ID)}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}

func (h *Handler) HandleStartGameRequest(message []byte, conn net.Conn) ([]byte, bool) {
	var dto StartGameRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid START_GAME payload"), false
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	err := h.RoomController.StartGame(player, dto.RoomID)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}

	response := map[string]string{"message": "Game started successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}

func (h *Handler) HandleJoinRoomRequest(message []byte, conn net.Conn) ([]byte, bool) {
	var dto JoinRoomDTO
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid JOIN_ROOM payload"), false
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	room, err := h.RoomController.JoinRoom(player, dto.RoomID, dto.Password)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	responseBytes, err := json.Marshal(room)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}

func (h *Handler) HandleDeleteRoomRequest(message []byte, conn net.Conn) ([]byte, bool) {
	var dto DeleteRoomDTO
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid DELETE_ROOM payload"), false
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	err := h.RoomController.DeleteRoom(player, dto.RoomID)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	response := map[string]string{"message": "Room deleted successfully"}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}

func (h *Handler) HandleGuessLetterRequest(message []byte, conn net.Conn) ([]byte, bool) {
	var dto GuessLetterRequest
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid GUESS_LETTER payload"), false
	}

	// Проверяем длину введённого символа
	if len(dto.Letter) != 1 {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid letter input. Please provide a single character."), false
	}

	player := &domain.Player{
		Username: dto.PlayerUsername,
		Conn:     conn,
	}

	isCorrect, feedback, err := h.RoomController.MakeGuess(player, dto.RoomID, []rune(dto.Letter)[0])
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}

	// Формируем успешный ответ
	response := GuessLetterResponse{
		IsCorrect: isCorrect,
		Feedback:  feedback,
	}

	// Проверяем окончание игры
	if strings.Contains(feedback, "Game Over") || strings.Contains(feedback, "Congratulations") {
		response.GameOver = true
	} else {
		response.GameOver = false
	}

	responseBytes, err := json.Marshal(response)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}

func (h *Handler) HandleGetGameStateRequest(message []byte) ([]byte, bool) {
	var dto GetGameStateDTO
	if err := json.Unmarshal(message, &dto); err != nil {
		return createErrorResponse(ErrCodeInvalidJSON, "Invalid GET_GAME_STATE payload"), false
	}
	response, err := h.RoomController.GetGameState(dto.RoomID)
	responseBytes, err := json.Marshal(response)
	if err != nil {
		return createErrorResponse(ErrCodeInternalServerError, err.Error()), false
	}
	return responseBytes, true
}
