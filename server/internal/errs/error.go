package errs

import "fmt"

// Error структура для представления ошибок
type Error struct {
	Code    int32  `json:"code"`
	Message string `json:"message"`
}

// Error реализует интерфейс error
func (e *Error) Error() string {
	return fmt.Sprintf("Code: %d, Message: %s", e.Code, e.Message)
}

// NewError создает новую ошибку
func NewError(code int32, message string) *Error {
	return &Error{
		Code:    code,
		Message: message,
	}
}
