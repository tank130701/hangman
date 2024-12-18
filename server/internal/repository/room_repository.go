package repository

import (
	"errors"
	"hangman/internal/domain"
	"sync"
)

type InMemoryRoomRepository struct {
	rooms map[string]*domain.Room
	mu    sync.RWMutex
}

func NewRoomRepository() domain.IRoomRepository {
	return &InMemoryRoomRepository{
		rooms: make(map[string]*domain.Room),
	}
}

func (r *InMemoryRoomRepository) AddRoom(room *domain.Room) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.rooms[room.ID]; exists {
		return errors.New("room already exists")
	}

	r.rooms[room.ID] = room
	return nil
}

func (r *InMemoryRoomRepository) UpdateRoom(room *domain.Room) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	// Проверяем, существует ли комната
	if _, exists := r.rooms[room.ID]; !exists {
		return errors.New("room not found")
	}

	// Обновляем комнату
	r.rooms[room.ID] = room
	return nil
}

func (r *InMemoryRoomRepository) GetRoomByID(roomID string) (*domain.Room, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()

	room, exists := r.rooms[roomID]
	if !exists {
		return nil, errors.New("room not found")
	}

	return room, nil
}

func (r *InMemoryRoomRepository) RemoveRoom(roomID string) error {
	r.mu.Lock()
	defer r.mu.Unlock()

	if _, exists := r.rooms[roomID]; !exists {
		return errors.New("room not found")
	}

	delete(r.rooms, roomID)
	return nil
}

func (r *InMemoryRoomRepository) GetAllRooms() []*domain.Room {
	r.mu.RLock()
	defer r.mu.RUnlock()

	rooms := make([]*domain.Room, 0, len(r.rooms))
	for _, room := range r.rooms {
		rooms = append(rooms, room)
	}

	return rooms
}
