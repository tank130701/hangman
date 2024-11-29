package service

import (
	"errors"
	"hangman/internal/domain"
	"time"
)

type RoomController struct {
	roomRepo    domain.IRoomRepository
	playerRepo  domain.IPlayerRepository
	gameService domain.IGameService
}

func NewRoomController(roomRepo domain.IRoomRepository, playerRepo domain.IPlayerRepository, gameService domain.IGameService) *RoomController {
	return &RoomController{
		roomRepo:    roomRepo,
		playerRepo:  playerRepo,
		gameService: gameService,
	}
}

func (rc *RoomController) CreateRoom(player *domain.Player, roomID, password string) (*domain.Room, error) {
	room := &domain.Room{
		ID:           roomID,
		Owner:        player,
		Players:      []*domain.Player{player},
		Password:     password,
		LastActivity: time.Now(),
		IsOpen:       true,
		MaxPlayers:   5,
	}

	if err := rc.roomRepo.AddRoom(room); err != nil {
		return nil, err
	}

	if err := rc.playerRepo.AddPlayer(player); err != nil {
		err = rc.roomRepo.RemoveRoom(roomID)
		return nil, err
	}

	return room, nil
}

func (rc *RoomController) JoinRoom(player *domain.Player, roomID, password string) (*domain.Room, error) {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return nil, err
	}

	if room.Password != "" && room.Password != password {
		return nil, errors.New("incorrect password")
	}

	room.Players = append(room.Players, player)
	room.LastActivity = time.Now()

	if err := rc.playerRepo.AddPlayer(player); err != nil {
		return nil, err
	}

	return room, nil
}

func (rc *RoomController) LeaveRoom(player *domain.Player, roomID string) error {
	room, err := rc.roomRepo.GetRoomByID(roomID)
	if err != nil {
		return err
	}

	// Удаляем игрока из комнаты
	for i, p := range room.Players {
		if p.Username == player.Username {
			room.Players = append(room.Players[:i], room.Players[i+1:]...)
			break
		}
	}

	// Если игрок — владелец, переназначаем нового
	if room.Owner.Username == player.Username && len(room.Players) > 0 {
		room.Owner = room.Players[0]
	}

	// Удаляем комнату, если она пуста
	if len(room.Players) == 0 {
		rc.roomRepo.RemoveRoom(roomID)
	}

	rc.playerRepo.RemovePlayer(player.Username)
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
	for _, p := range room.Players {
		rc.playerRepo.RemovePlayer(p.Username)
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

func (rc *RoomController) GetGameState(roomID string) (*domain.GameState, error) {
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
	if len(room.Players) == 0 {
		if err := rc.roomRepo.RemoveRoom(roomID); err != nil {
			return err
		}
		return nil
	}

	// Переназначаем владельца (первый оставшийся игрок становится владельцем)
	room.Owner = room.Players[0]

	// Обновляем комнату в репозитории
	if err := rc.roomRepo.UpdateRoom(room); err != nil {
		return err
	}

	return nil
}

func (rc *RoomController) JoinRandomRoom(player *domain.Player) (*domain.Room, error) {
	rooms := rc.roomRepo.GetAllRooms()

	for _, room := range rooms {
		if room.IsOpen && len(room.Players) < room.MaxPlayers {
			return rc.JoinRoom(player, room.ID, "")
		}
	}

	return nil, errors.New("no available rooms")
}
