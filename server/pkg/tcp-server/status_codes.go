package tcp_server

const (
	StatusSuccess             = 2000 // Успех
	StatusBadRequest          = 4000 // Некорректный формат
	StatusNotFound            = 4004 // Неизвестная команда
	StatusUnauthorized        = 4003 // Неавторизованное действие
	StatusConflict            = 4009 // Комната уже существует
	StatusInternalServerError = 5000 // Ошибка на сервере
)
