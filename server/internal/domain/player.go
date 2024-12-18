package domain

import (
	"net"
	"time"
)

type Player struct {
	Username    string
	Score       int
	Conn        net.Conn
	IsConnected bool      // Поле для проверки подключения
	LastActive  time.Time // Время последней активности
}

func NewPlayer(
	name string,
	score int,
	conn net.Conn,
) *Player {
	return &Player{
		Username:    name,
		Score:       score,
		Conn:        conn,
		IsConnected: true,       // Новый игрок подключён по умолчанию
		LastActive:  time.Now(), // Инициализируем время активности
	}
}
