package service

import (
	"errors"
	"hangman/internal/domain"
	"net"
	"time"
)

type RoomController struct {
	roomRepo    domain.IRoomRepository
	gameService domain.IGameService
	playersRepo domain.IPlayerRepository
}

func NewRoomController(
	roomRepo domain.IRoomRepository,
	playersRepo domain.IPlayerRepository,
	gameService domain.IGameService,
) *RoomController {
	return &RoomController{
		roomRepo:    roomRepo,
		playersRepo: playersRepo,
		gameService: gameService,
	}
}

func (rc *RoomController) CreateRoom(player string, roomID, password, category, difficulty string) (*domain.Room, error) {
	room := &domain.Room{
		ID:      roomID,
		Owner:   &player,
		Players: make(map[string]*domain.Player),
		//StateManager: domain.NewGameStateManager(),
		Password:     password,
		Category:     category,
		Difficulty:   difficulty,
		LastActivity: time.Now(),
		MaxPlayers:   5,
	}

	if err := rc.roomRepo.AddRoom(room); err != nil {
		return nil, err
	}

	//err := rc.playersRepo.AddPlayer(player)
	//if err != nil {
	//	return nil, err
	//}

	return room, nil
}

func (rc *RoomController) JoinRoom(conn net.Conn, username, roomID, password string) (*domain.Room, error) {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return nil, err
	}

	// Проверяем пароль
	if room.Password != "" && room.Password != password {
		return nil, errors.New("incorrect password")
	}

	player := domain.NewPlayer(username, 0, conn)
	connAddr := conn.RemoteAddr().String()
	newClientKey := domain.NewClientKey(connAddr, username, password)

	err = rc.playersRepo.AddPlayer(newClientKey, player)
	if err != nil {
		return nil, err
	}
	room.AddPlayer(player)
	return room, nil
}

func (rc *RoomController) LeaveRoom(clientKey domain.ClientKey, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err
	}
	player, err := rc.playersRepo.GetPlayerByKey(clientKey)
	if err != nil {
		return err
	}
	room.RemovePlayer(player.Username)                           // Удаление из комнаты
	err = rc.playersRepo.RemovePlayerByUsername(player.Username) // Удаление из глобального репозитория
	if err != nil {
		return err
	}
	return nil
}

func (rc *RoomController) DeleteRoom(clientKey domain.ClientKey, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err // Комната не найдена
	}

	player, err := rc.playersRepo.GetPlayerByKey(clientKey)
	if err != nil {
		return err
	}

	// Проверяем, является ли пользователь владельцем комнаты
	if *room.Owner != player.Username {
		return errors.New("only the owner can delete the room")
	}
	// Удаляем всех игроков из комнаты
	for _, player := range room.GetAllPlayers() {
		err := rc.playersRepo.RemovePlayerByUsername(player)
		if err != nil {
			return err
		}
	}

	// Удаляем комнату из репозитория
	if err := rc.roomRepo.RemoveRoom(roomID); err != nil {
		return err
	}

	return nil
}

func (rc *RoomController) forceDeleteRoom(roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err // Комната не найдена
	}

	room.RLock()
	players := room.GetAllPlayers()
	room.RUnlock()

	for _, player := range players {
		err := rc.playersRepo.RemovePlayerByUsername(player)
		if err != nil {
			return err
		}
	}

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
				rc.forceDeleteRoom(room.ID)
			}
		}
	}
}

func (rc *RoomController) StartGame(clientKey domain.ClientKey, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err
	}
	//room.Lock()
	//defer room.Unlock()
	player, err := rc.playersRepo.GetPlayerByKey(clientKey)
	if err != nil {
		return err
	}
	if *room.Owner != player.Username {
		return errors.New("only the owner can start the game")
	}

	return rc.gameService.StartGame(room)
}

func (rc *RoomController) MakeGuess(clientKey domain.ClientKey, roomID string, letter rune) (bool, string, error) {
	player, err := rc.playersRepo.GetPlayerByKey(clientKey)
	if err != nil {
		return false, "", err
	}
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
	if room.GetPlayerCount() == 0 {
		if err := rc.roomRepo.RemoveRoom(roomID); err != nil {
			return err
		}
		return nil
	}

	// Переназначаем владельца (первый оставшийся игрок становится владельцем)
	room.Owner = &room.GetAllPlayers()[0]

	// Обновляем комнату в репозитории
	if err := rc.roomRepo.UpdateRoom(room); err != nil {
		return err
	}

	return nil
}

func (rc *RoomController) GetAllRooms() ([]*domain.Room, error) {
	return rc.roomRepo.GetAllRooms(), nil
}

func (rc *RoomController) GetPlayerUsernamesAndScores() map[string]int {
	return rc.playersRepo.GetPlayerUsernamesAndScores()
}
