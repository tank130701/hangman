package main

import (
	"fmt"
	"hangman/internal/repository"
	"hangman/internal/service"
	"hangman/internal/transport/tcp"
	"hangman/pkg/tcp-server"
	"hangman/pkg/utils"
)

func main() {
	// Логгер
	logger := utils.NewCustomLogger()
	// Репозитории
	roomRepo := repository.NewRoomRepository()
	playerRepo := repository.NewPlayerRepository()

	// Сервисы
	gameService := service.NewGameService()
	roomController := service.NewRoomController(roomRepo, playerRepo, gameService)

	// Обработчики
	handler := tcp.NewHandler(roomController)

	// Запускаем процесс очистки неактивных комнат
	go func() {
		timeout := 300 // Таймаут удаления комнат в секундах
		logger.Info(fmt.Sprintf("Room cleanup process started with timeout: %d seconds", timeout))
		roomController.CleanupRooms(timeout)
	}()

	// Создаём и запускаем TCP-сервер
	srv := tcp_server.New(":8001", logger) // Передаем RoomController и Logger

	handler.InitRoutes(srv)

	if err := srv.Start(); err != nil {
		logger.Error(fmt.Sprintf("Failed to start server: %v", err))
	}
}
