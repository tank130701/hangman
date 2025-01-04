package main

import (
	"fmt"
	"hangman/internal/repository"
	"hangman/internal/service"
	"hangman/internal/transport/tcp"
	ctx_repo "hangman/pkg/ctx-repo"
	"hangman/pkg/tcp-server"
	"hangman/pkg/utils"
	"os"
	"time"
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
	ctxRepo := ctx_repo.NewCtxRepository()
	// Сервисы
	gameService := service.NewGameService(wordsRepo)
	roomController := service.NewRoomController(roomRepo, playerRepo, gameService, ctxRepo)

	// Обработчики
	handler := tcp.NewHandler(roomController)

	// Канал для передачи неактивных игроков
	inactivePlayersChan := make(chan []string)

	// Запускаем процесс мониторинга соединений
	go func() {
		timeout := 2 * time.Minute // Таймаут проверки неактивных игроков
		logger.Info(fmt.Sprintf("Player inactivity monitoring started with timeout: %v", timeout))
		playerRepo.MonitorConnections(timeout, inactivePlayersChan)
	}()

	// Запускаем обработку неактивных игроков
	go func() {
		for inactivePlayers := range inactivePlayersChan {
			if len(inactivePlayers) > 0 {
				logger.Info(fmt.Sprintf("Inactive players detected: %v", inactivePlayers))
				// Дополнительная логика для обработки неактивных игроков
			}
		}
	}()

	// Запускаем процесс очистки неактивных комнат
	go func() {
		timeout := 60 // Таймаут удаления комнат в секундах
		logger.Info(fmt.Sprintf("Room cleanup process started with timeout: %d seconds", timeout))
		roomController.CleanupRooms(timeout)
	}()

	// Создаём и запускаем TCP-сервер
	srv := tcp_server.New(":8001", ctxRepo, logger) // Передаем RoomController и Logger
	handler.InitRoutes(srv)
	if err := srv.Start(); err != nil {
		logger.Fatal(fmt.Sprintf("Failed to start server: %v", err))
	}
}
