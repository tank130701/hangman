package tcp

import "encoding/json"

// createSuccessResponse создает JSON для успешного ответа
func createSuccessResponse(payload []byte) []byte {
	response := Response{
		Status:  "success",
		Payload: payload,
	}
	respJSON, _ := json.Marshal(response)
	return respJSON
}

// createErrorResponse создает JSON для ошибки
func createErrorResponse(code int, message string) []byte {
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
