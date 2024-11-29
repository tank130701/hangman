package tcp

import (
	"encoding/json"
	"net"
)

// processMessage обрабатывает JSON-команды
func (s *Server) processMessage(message []byte, conn net.Conn) []byte {
	// Логируем входящий запрос
	s.logger.Printf("Received message: %s", string(message))

	var request BaseRequest
	if err := json.Unmarshal(message, &request); err != nil {
		errMessage := "Invalid JSON format"
		s.logger.Printf("Error parsing request: %v", err)
		return createErrorResponse(4000, errMessage)
	}

	var response []byte
	switch request.Command {
	case "CREATE_ROOM":
		response = s.handler.HandleCreateRoomRequest(message, conn)
	case "START_GAME":
		response = s.handler.HandleStartGameRequest(message, conn)
	case "JOIN_ROOM":
		response = s.handler.HandleJoinRoomRequest(message, conn)
	case "DELETE_ROOM":
		response = s.handler.HandleDeleteRoomRequest(message, conn)
	case "GUESS_LETTER":
		response = s.handler.HandleGuessLetterRequest(message, conn)
	case "GET_GAME_STATE":
		response = s.handler.HandleGetGameStateRequest(message)
	default:
		unknownCommandMessage := "Unknown command"
		s.logger.Printf("Unknown command: %s", request.Command)
		return createErrorResponse(ErrCodeUnknownCommand, unknownCommandMessage)
	}

	// Логируем ответ
	var debugData map[string]interface{}
	if err := json.Unmarshal(response, &debugData); err != nil {
		s.logger.Printf("Failed to parse response JSON: %v", err)
	} else {
		prettyResponse, _ := json.MarshalIndent(debugData, "", "  ") // Форматирование JSON для читаемости
		s.logger.Printf("Response JSON:\n%s", prettyResponse)
	}

	return response
}
