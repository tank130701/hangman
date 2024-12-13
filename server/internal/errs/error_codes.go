package errs

const (
	// Общие ошибки
	ErrCodeInvalidJSON    = 4000 // Некорректный формат JSON
	ErrCodeUnknownCommand = 4001 // Неизвестная команда
	ErrCodeUnauthorized   = 4002 // Неавторизованное действие

	// Ошибки комнат
	ErrCodeRoomAlreadyExists = 4100 // Комната уже существует
	ErrCodeRoomNotFound      = 4101 // Комната не найдена
	ErrCodeRoomFull          = 4102 // Комната переполнена

	// Ошибки игры
	ErrCodeGameNotStarted = 4200 // Игра не начата
	ErrCodeGameOver       = 4201 // Игра завершена
	ErrCodeInvalidMove    = 4202 // Некорректный ход

	// Ошибки сервера
	ErrCodeInternalServerError = 5000 // Ошибка на сервере
)
