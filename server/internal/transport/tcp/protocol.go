package tcp

import (
	"encoding/json"
	"net"
)

// Response представляет стандартный ответ сервера
type Response struct {
	Status  string `json:"status"`          // "success" или "error"
	Payload []byte `json:"data,omitempty"`  // Данные ответа в случае успеха
	Error   *Error `json:"error,omitempty"` // Ошибка в случае неуспеха
}

// Error представляет структуру ошибки
type Error struct {
	Code    int    `json:"code"`    // Код ошибки
	Message string `json:"message"` // Описание ошибки
}

// processMessage обрабатывает JSON-команды
func (s *Server) processMessage(message []byte, conn net.Conn) []byte {
	var request BaseRequest
	if err := json.Unmarshal(message, &request); err != nil {
		return createErrorResponse(4000, "Invalid JSON format")
	}

	var response []byte
	var success bool
	switch request.Command {
	case "CREATE_ROOM":
		response, success = s.handler.HandleCreateRoomRequest(message, conn)
	case "START_GAME":
		response, success = s.handler.HandleStartGameRequest(message, conn)
	case "JOIN_ROOM":
		response, success = s.handler.HandleJoinRoomRequest(message, conn)
	case "DELETE_ROOM":
		response, success = s.handler.HandleDeleteRoomRequest(message, conn)
	case "GUESS_LETTER":
		response, success = s.handler.HandleGuessLetterRequest(message, conn)
	case "GET_GAME_STATE":
		response, success = s.handler.HandleGetGameStateRequest(message)

	default:
		return createErrorResponse(ErrCodeUnknownCommand, "Unknown command")
	}

	if success {
		return createSuccessResponse(response)
	}

	return response
}
