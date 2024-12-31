package main

import (
	"fmt"
	"hangman/internal/repository"
	"hangman/internal/service"
	"hangman/internal/transport/tcp"
	"hangman/pkg/tcp-server"
	"hangman/pkg/utils"
	"os"
)

func main() {
	// Логгер
	logger := utils.NewCustomLogger(utils.LevelDebug)
	// Репозитории
	roomRepo := repository.NewRoomRepository()
	wordsRepo, err := repository.NewWordsRepository("../assets" + string(os.PathSeparator) + "words.json")
	if err != nil {
		logger.Fatal(fmt.Sprintf("Failed to init words repo: %v", err))
	}
	playerRepo := repository.NewPlayerRepository()
	// Сервисы
	gameService := service.NewGameService(wordsRepo)
	roomController := service.NewRoomController(roomRepo, playerRepo, gameService)

	// Обработчики
	handler := tcp.NewHandler(roomController)

	// Запускаем процесс очистки неактивных комнат
	go func() {
		timeout := 60 // Таймаут удаления комнат в секундах
		logger.Info(fmt.Sprintf("Room cleanup process started with timeout: %d seconds", timeout))
		roomController.CleanupRooms(timeout)
	}()

	// Создаём и запускаем TCP-сервер
	srv := tcp_server.New(":8001", logger) // Передаем RoomController и Logger
	handler.InitRoutes(srv)
	if err := srv.Start(); err != nil {
		logger.Fatal(fmt.Sprintf("Failed to start server: %v", err))
	}

	notificationSrv := tcp_server.NewNotificationServer(":8002", logger)
	if err := notificationSrv.Start(); err != nil {
		logger.Fatal(fmt.Sprintf("Failed to start notification server: %v", err))
	}
}
