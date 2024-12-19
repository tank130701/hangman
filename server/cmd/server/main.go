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
	playerRepo := repository.NewPlayerRepository()
	wordsRepo, err := repository.NewWordsRepository("../assets" + string(os.PathSeparator) + "words.json")
	if err != nil {
		logger.Fatal(fmt.Sprintf("Failed to init words repo: %v", err))
	}

	// Сервисы
	gameService := service.NewGameService(wordsRepo)
	roomController := service.NewRoomController(roomRepo, playerRepo, gameService)

	// Обработчики
	handler := tcp.NewHandler(roomController)

	// Запускаем процесс очистки неактивных комнат
	go func() {
		timeout := 300 // Таймаут удаления комнат в секундах
		logger.Info(fmt.Sprintf("Room cleanup process started with timeout: %d seconds", timeout))
		roomController.CleanupRooms(timeout)
	}()

	//go func() {
	//	ticker := time.NewTicker(10 * time.Second) // Проверяем каждые 10 секунд
	//	defer ticker.Stop()
	//
	//	for range ticker.C {
	//		disconnectedPlayers := playerRepo.CheckConnections(30 * time.Second) // Таймаут — 30 секунд
	//
	//		// Логируем отключённых игроков
	//		for _, player := range disconnectedPlayers {
	//			logger.Info(fmt.Sprintf("Игрок %s был удалён из-за таймаута реконнекта", player))
	//		}
	//	}
	//}()

	// Создаём и запускаем TCP-сервер
	srv := tcp_server.New(":8001", logger) // Передаем RoomController и Logger

	handler.InitRoutes(srv)

	if err := srv.Start(); err != nil {
		logger.Fatal(fmt.Sprintf("Failed to start server: %v", err))
	}
}
