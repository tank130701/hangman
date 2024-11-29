package tcp

import (
	"encoding/json"
)

// Response представляет универсальный ответ сервера
type Response struct {
	Code    int             `json:"code"`              // Код ответа (например, 200, 400, 500)
	Payload json.RawMessage `json:"payload,omitempty"` // Данные ответа в случае успеха
	Error   *Error          `json:"error,omitempty"`   // Описание ошибки
}

// Error представляет структуру ошибки
type Error struct {
	Message string `json:"message"` // Описание ошибки
}

// CreateSuccessfulResponse создает JSON-ответ для успешного запроса
func createSuccessfulResponse(payload []byte) []byte {
	// Код ответа для успешных операций
	response := Response{
		Code:    2000,
		Payload: payload,
	}
	respJSON, _ := json.Marshal(response)
	return respJSON
}

// CreateErrorResponse создает JSON-ответ для ошибки
func createErrorResponse(code int, message string) []byte {
	response := Response{
		Code: code,
		Error: &Error{
			Message: message,
		},
	}
	respJSON, _ := json.Marshal(response)
	return respJSON
}
