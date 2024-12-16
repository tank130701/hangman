package service

import (
	"errors"
	"hangman/internal/domain"
	"hangman/internal/repository"
	"time"
)

type RoomController struct {
	roomRepo    domain.IRoomRepository
	gameService domain.IGameService
}

func NewRoomController(roomRepo domain.IRoomRepository, gameService domain.IGameService) *RoomController {
	return &RoomController{
		roomRepo:    roomRepo,
		gameService: gameService,
	}
}

func (rc *RoomController) CreateRoom(player *domain.Player, roomID, password, category, difficulty string) (*domain.Room, error) {
	room := &domain.Room{
		ID:          roomID,
		Owner:       player,
		PlayersRepo: repository.NewPlayerRepository(),
		//StateManager: domain.NewGameStateManager(),
		Password:     password,
		Category:     category,
		Difficulty:   difficulty,
		LastActivity: time.Now(),
		IsOpen:       true,
		MaxPlayers:   5,
	}

	if err := rc.roomRepo.AddRoom(room); err != nil {
		return nil, err
	}

	//err := room.PlayersRepo.AddPlayer(player)
	//if err != nil {
	//	return nil, err
	//}

	return room, nil
}

func (rc *RoomController) JoinRoom(player *domain.Player, roomID, password string) (*domain.Room, error) {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return nil, err
	}

	// Проверяем пароль
	if room.Password != "" && room.Password != password {
		return nil, errors.New("incorrect password")
	}

	err = room.PlayersRepo.AddPlayer(player)
	if err != nil {
		return nil, err
	}

	return room, nil
}

func (rc *RoomController) LeaveRoom(player *domain.Player, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err
	}

	err = room.PlayersRepo.RemovePlayer(player.Username)
	if err != nil {
		return err
	}
	return nil
}

func (rc *RoomController) DeleteRoom(player *domain.Player, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err // Комната не найдена
	}

	// Проверяем, является ли пользователь владельцем комнаты
	if room.Owner.Username != player.Username {
		return errors.New("only the owner can delete the room")
	}
	// Удаляем всех игроков из комнаты
	for _, p := range room.PlayersRepo.GetAllPlayers() {
		err := room.PlayersRepo.RemovePlayer(p.Username)
		if err != nil {
			return err
		} //TODO: check this
	}

	// Удаляем комнату из репозитория
	if err := rc.roomRepo.RemoveRoom(roomID); err != nil {
		return err
	}

	return nil
}

func (rc *RoomController) CleanupRooms(timeoutSeconds int) {
	ticker := time.NewTicker(time.Second * time.Duration(timeoutSeconds))
	defer ticker.Stop()

	for range ticker.C {
		rooms := rc.roomRepo.GetAllRooms()
		for _, room := range rooms {
			if time.Since(room.LastActivity) > time.Duration(timeoutSeconds)*time.Second {
				rc.roomRepo.RemoveRoom(room.ID)
			}
		}
	}
}

func (rc *RoomController) StartGame(player *domain.Player, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err
	}

	if room.Owner.Username != player.Username {
		return errors.New("only the owner can start the game")
	}

	return rc.gameService.StartGame(room)
}

func (rc *RoomController) MakeGuess(player *domain.Player, roomID string, letter rune) (bool, string, error) {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return false, "", err
	}

	return rc.gameService.MakeGuess(room, player, letter)
}

func (rc *RoomController) GetGameState(roomID string) (map[string]*domain.GameState, error) {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return nil, err
	}

	return rc.gameService.GetGameState(room)
}

func (rc *RoomController) HandleOwnerChange(roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err // Комната не найдена
	}

	// Если в комнате никого не осталось, удаляем её
	if room.PlayersRepo.GetPlayerCount() == 0 {
		if err := rc.roomRepo.RemoveRoom(roomID); err != nil {
			return err
		}
		return nil
	}

	// Переназначаем владельца (первый оставшийся игрок становится владельцем)
	room.Owner = room.PlayersRepo.GetAllPlayers()[0]

	// Обновляем комнату в репозитории
	if err := rc.roomRepo.UpdateRoom(room); err != nil {
		return err
	}

	return nil
}

func (rc *RoomController) JoinRandomRoom(player *domain.Player) (*domain.Room, error) {
	rooms := rc.roomRepo.GetAllRooms()

	for _, room := range rooms {
		if room.IsOpen && room.PlayersRepo.GetPlayerCount() < room.MaxPlayers {
			return rc.JoinRoom(player, room.ID, "")
		}
	}

	return nil, errors.New("no available rooms")
}

func (rc *RoomController) GetAllRooms() ([]*domain.Room, error) {
	return rc.roomRepo.GetAllRooms(), nil
}
