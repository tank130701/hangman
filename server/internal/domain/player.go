package domain

import (
	"context"
	"net"
	"time"
)

type Player struct {
	Username    string
	Score       int
	Conn        *net.Conn
	IsConnected bool      // Поле для проверки подключения
	LastActive  time.Time // Время последней активности
}

func NewPlayer(
	ctx context.Context,
	conn *net.Conn,
	name string,
	score int,
) *Player {
	return &Player{
		Username:    name,
		Score:       score,
		Conn:        conn,
		IsConnected: true,       // Новый игрок подключён по умолчанию
		LastActive:  time.Now(), // Инициализируем время активности
	}
}
