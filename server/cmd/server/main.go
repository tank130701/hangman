package main

import (
	"hangman/internal/repository"
	"hangman/internal/service"
	"hangman/internal/transport/tcp"
	"hangman/pkg/utils"
)

func main() {
	// Логгер
	logger := utils.Logger
	// Репозитории
	roomRepo := repository.NewRoomRepository()
	playerRepo := repository.NewPlayerRepository()

	// Сервисы
	gameService := service.NewGameService()
	roomController := service.NewRoomController(roomRepo, playerRepo, gameService)

	// Обработчики
	handlers := tcp.NewHandler(roomController)

	// Запускаем процесс очистки неактивных комнат
	go func() {
		timeout := 300 // Таймаут удаления комнат в секундах
		logger.Printf("Room cleanup process started with timeout: %d seconds", timeout)
		roomController.CleanupRooms(timeout)
	}()

	// Создаём и запускаем TCP-сервер
	srv := tcp.New(":8001", handlers, logger) // Передаем RoomController и Logger
	if err := srv.Start(); err != nil {
		logger.Fatalf("Failed to start server: %v", err)
	}
}
