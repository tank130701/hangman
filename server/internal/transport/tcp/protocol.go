package tcp

import (
	"encoding/json"
	"hangman/internal/domain"
	"strings"
)

// Response представляет стандартный ответ сервера
type Response struct {
	Status string      `json:"status"`          // "success" или "error"
	Data   interface{} `json:"data,omitempty"`  // Данные ответа в случае успеха
	Error  *Error      `json:"error,omitempty"` // Ошибка в случае неуспеха
}

// Error представляет структуру ошибки
type Error struct {
	Code    int    `json:"code"`    // Код ошибки
	Message string `json:"message"` // Описание ошибки
}

// processMessage обрабатывает JSON-команды
func (s *Server) processMessage(message []byte, player *domain.Player) ([]byte, error) {
	var request BaseRequest
	if err := json.Unmarshal(message, &request); err != nil {
		return s.createErrorResponse(4000, "Invalid JSON format"), nil
	}

	var response interface{}
	var err error

	switch request.Command {
	case "CREATE_ROOM":
		var dto CreateRoomDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4001, "Invalid CREATE_ROOM payload"), nil
		}
		response, err = s.RoomController.CreateRoom(player, dto.RoomID, dto.Password)

	case "START_GAME":
		var dto StartGameDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4006, "Invalid START_GAME payload"), nil
		}
		err := s.RoomController.StartGame(player, dto.RoomID)
		if err != nil {
			return s.createErrorResponse(5000, err.Error()), nil
		}
		return s.createSuccessResponse(map[string]string{"message": "Game started successfully"}), nil
	case "JOIN_ROOM":
		var dto JoinRoomDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4002, "Invalid JOIN_ROOM payload"), nil
		}
		response, err = s.RoomController.JoinRoom(player, dto.RoomID, dto.Password)
	case "DELETE_ROOM":
		var dto DeleteRoomDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4006, "Invalid DELETE_ROOM payload"), nil
		}
		err = s.RoomController.DeleteRoom(player, dto.RoomID)
		if err != nil {
			return s.createErrorResponse(5000, err.Error()), nil
		}
		response = map[string]string{"message": "Room deleted successfully"}
	case "GUESS_LETTER":
		var dto GuessLetterDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4003, "Invalid GUESS_LETTER payload"), nil
		}

		// Проверяем длину введённого символа
		if len(dto.Letter) != 1 {
			return s.createErrorResponse(4004, "Invalid letter input. Please provide a single character."), nil
		}

		isCorrect, feedback, err := s.RoomController.MakeGuess(player, dto.RoomID, []rune(dto.Letter)[0])
		if err != nil {
			return s.createErrorResponse(5000, err.Error()), nil
		}

		// Формируем успешный ответ
		responseData := map[string]interface{}{
			"is_correct": isCorrect,
			"feedback":   feedback,
		}

		// Проверяем окончание игры
		if strings.Contains(feedback, "Game Over") || strings.Contains(feedback, "Congratulations") {
			responseData["game_over"] = true
		} else {
			responseData["game_over"] = false
		}

		return s.createSuccessResponse(responseData), nil

	case "GET_GAME_STATE":
		var dto GetGameStateDTO
		if err := json.Unmarshal(message, &dto); err != nil {
			return s.createErrorResponse(4004, "Invalid GET_GAME_STATE payload"), nil
		}
		response, err = s.RoomController.GetGameState(dto.RoomID)

	default:
		return s.createErrorResponse(4005, "Unknown command"), nil
	}

	if err != nil {
		return s.createErrorResponse(5000, err.Error()), nil
	}

	return s.createSuccessResponse(response), nil
}

// createSuccessResponse создает JSON для успешного ответа
func (s *Server) createSuccessResponse(data interface{}) []byte {
	response := Response{
		Status: "success",
		Data:   data,
	}
	respJSON, _ := json.Marshal(response)
	return respJSON
}

// createErrorResponse создает JSON для ошибки
func (s *Server) createErrorResponse(code int, message string) []byte {
	response := Response{
		Status: "error",
		Error: &Error{
			Code:    code,
			Message: message,
		},
	}
	respJSON, _ := json.Marshal(response)
	return respJSON
}
